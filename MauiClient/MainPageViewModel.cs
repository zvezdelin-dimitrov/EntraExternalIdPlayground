using Microsoft.Extensions.Configuration;
using MsalClientLib;
using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiClient;

public record KeyValue(string Key, string Value);

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string apiResult;

    [ObservableProperty]
    private IEnumerable<KeyValue> userData;

    [ObservableProperty]
    private bool loggedIn;

    public bool LoggedOut => !LoggedIn; // There is no built-in InvertedBoolConverter

    partial void OnLoggedInChanged(bool value) => OnPropertyChanged(nameof(LoggedOut));

    private readonly HttpClient httpClient = new();
    private readonly string apiUrl;

    public MainPageViewModel(IConfiguration configuration)
    {
        apiUrl = configuration["ZvezdoApiEndpoint"];
        Initialize();
    }

    private async void Initialize()
    {
        var cachedUserAccount = await PublicClientSingleton.Instance.MSALClientHelper.FetchSignedInUserFromCache();
        if (cachedUserAccount is not null)
        {
            await Authenticate();
        }
    }

    private async Task Authenticate()
    {
        // This will launch authentication in the default browser if cachedUserAccount is null
        var token = await PublicClientSingleton.Instance.AcquireTokenSilentAsync();
        if (token is null)
        {
            return;
        }

        var expires = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.ExpiresOn.ToLocalTime();
        var scopes = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.Scopes.ToList();
        var claims = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.ClaimsPrincipal.Claims.ToList();

        // In order for this to work, the optional email claim for ID TOKEN should be added in token configuration
        var email = claims.SingleOrDefault(x => x.Type == "email")?.Value;

        // This usually is the email and comes from "preferred_username" claim
        var name = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.Account.Username;

        // Should be added if needed during registration or from an admin later
        var nameFromClaim = claims.SingleOrDefault(x => x.Type == "name")?.Value;

        UserData = new List<KeyValue>
        {
            
            new("expires", expires.ToString()),
            new("scopes", string.Join(", ", scopes.Select(x => x.Split('/').Last()))),
            new("email", email),
            new("name", name),
            new("nameFromClaim", nameFromClaim),
        };

        LoggedIn = true;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [RelayCommand]
    private async Task SignIn()
    {
        await Authenticate();

#if WINDOWS
        // Hack to activate the application
        // Check if the new ActivateWindow in .NET 9 will work
        WinUI.App.Activate();
#endif
    }

    [RelayCommand]
    private async Task SignOut()
    {
        await PublicClientSingleton.Instance.SignOutAsync();

        UserData = null;
        LoggedIn = false;
    }

    [RelayCommand]
    private async Task CallApi()
    {
        var response = await httpClient.GetAsync(apiUrl);
        var responseContent = await response.Content.ReadAsStringAsync();
        ApiResult = responseContent;
    }

    [RelayCommand]
    private async Task GetGraphData()
    {
        // In order for this to work, the User.Read Microsoft Graph should be added in API permissions
        var user = await PublicClientSingleton.Instance.MSGraphHelper.GetMeAsync();
        var displayNameFromGraph = user.DisplayName;
        var emailFromGraph = user.Mail;
        ApiResult = $"{nameof(displayNameFromGraph)}:{displayNameFromGraph}, {nameof(emailFromGraph)}:{emailFromGraph}";
    }    
}
