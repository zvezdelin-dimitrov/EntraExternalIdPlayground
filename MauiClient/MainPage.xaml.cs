using MsalClientLib;
using System.Net.Http.Headers;

namespace MauiClient;

public partial class MainPage : ContentPage
{
    private HttpClient httpClient = new();

    public MainPage()
    {
        // This initialization should be at application level, RedirectUri is platform specific
        PlatformConfig.Instance.RedirectUri = PublicClientSingleton.Instance.MSALClientHelper.AzureADConfig.RedirectURI;
        var existinguser = Task.Run(PublicClientSingleton.Instance.MSALClientHelper.InitializePublicClientAppAsync).Result;

        InitializeComponent();

        Authenticate();                
    }

    private async Task Authenticate()
    {
        var cachedUserAccount = await PublicClientSingleton.Instance.MSALClientHelper.FetchSignedInUserFromCache();

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

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async void CallApiClicked(object sender, EventArgs e)
    {
        // Extract url to config
        var response = await httpClient.GetAsync("https://localhost:44355/userdata");
        var responseContent = await response.Content.ReadAsStringAsync();
        ApiResult.Text = responseContent;
    }
}
