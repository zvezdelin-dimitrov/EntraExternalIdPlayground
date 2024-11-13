using MsalClientLib;
using System.Net.Http.Headers;

namespace MauiClient;

public partial class MainPage : ContentPage
{
    private readonly HttpClient httpClient = new();

    public MainPage()
    {
        InitializeComponent();

        var cachedUserAccount = PublicClientSingleton.Instance.MSALClientHelper.FetchSignedInUserFromCache().Result;
        if (cachedUserAccount is not null)
        {
            Authenticate();
        }
    }

    private async Task Authenticate()
    {
        // This will launch authentication in the default browser if cachedUserAccount is null
        var token = await PublicClientSingleton.Instance.AcquireTokenSilentAsync();

        var expires = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.ExpiresOn.ToLocalTime();
        var scopes = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.Scopes.ToList();
        var claims = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.ClaimsPrincipal.Claims.ToList();

        // In order for this to work, the optional email claim for ID TOKEN should be added in token configuration
        var email = claims.SingleOrDefault(x => x.Type == "email")?.Value;

        // This usually is the email and comes from "preferred_username" claim
        var name = PublicClientSingleton.Instance.MSALClientHelper.AuthResult.Account.Username;

        // Should be added if needed during registration or from an admin later
        var nameFromClaim = claims.SingleOrDefault(x => x.Type == "name")?.Value;

        UserData.ItemsSource = new Dictionary<string, string>
        {
            { "expires", expires.ToString() },
            { "scopes", string.Join(", ", scopes.Select(x => x.Split('/').Last())) },
            { "email", email },
            { "name", name },
            { "nameFromClaim", nameFromClaim },
        };

        CallApiButton.IsEnabled = true;
        SignInButton.IsEnabled = false;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async void SignInClicked(object sender, EventArgs e)
    {
        await Authenticate();

#if WINDOWS
        // Hack to activate the application
        // Check if the new ActivateWindow in .NET 9 will work
        WinUI.App.Activate();
#endif
    }

    private async void CallApiClicked(object sender, EventArgs e)
    {
        // Extract url to config
        var response = await httpClient.GetAsync("https://localhost:44355/userdata");
        var responseContent = await response.Content.ReadAsStringAsync();
        ApiResult.Text = responseContent;
    }
}
