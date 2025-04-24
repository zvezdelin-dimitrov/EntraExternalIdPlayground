using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using MSALClientLib.Graph;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MsalClientLib;

public class PublicClientSingleton
{
    /// <summary>
    /// This is the singleton used by Ux. Since PublicClientWrapper constructor does not have perf or memory issue, it is instantiated directly.
    /// </summary>
    public static PublicClientSingleton Instance { get; private set; } = new PublicClientSingleton();

    private static IConfiguration AppConfiguration;

    public MSALClientHelper MSALClientHelper { get; }

    public DownstreamApiHelper DownstreamApiHelper { get; }

    public MSGraphHelper MSGraphHelper { get; }
        
    public bool UseEmbedded { get; set; }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private PublicClientSingleton()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var embeddedConfigfilename = $"{assembly.GetName().Name}.msal.appsettings.json";
        using var stream = assembly.GetManifestResourceStream(embeddedConfigfilename);
        AppConfiguration = new ConfigurationBuilder().AddJsonStream(stream).Build();

        AzureADConfig azureADConfig = AppConfiguration.GetSection("AzureAD").Get<AzureADConfig>();
        MSALClientHelper = new MSALClientHelper(azureADConfig);

        DownStreamApiConfig downStreamApiConfig = AppConfiguration.GetSection("DownstreamApi").Get<DownStreamApiConfig>();
        DownstreamApiHelper = new DownstreamApiHelper(downStreamApiConfig, MSALClientHelper);

        MSGraphApiConfig graphApiConfig = AppConfiguration.GetSection("MSGraphApi").Get<MSGraphApiConfig>();
        MSGraphHelper = new MSGraphHelper(graphApiConfig, MSALClientHelper);

        UseEmbedded = AppConfiguration.GetValue<bool>("UseEmbeddedBrowser");
    }

    public async Task<string> AcquireTokenSilentAsync()
    {
        return await AcquireTokenSilentAsync(GetScopes()).ConfigureAwait(false);
    }

    public async Task<string> AcquireTokenSilentAsync(string[] scopes)
    {
        return await MSALClientHelper.SignInUserAndAcquireAccessToken(scopes).ConfigureAwait(false);
    }

    internal async Task<AuthenticationResult> AcquireTokenInteractiveAsync(string[] scopes)
    {
        MSALClientHelper.UseEmbedded = UseEmbedded;
        return await MSALClientHelper.SignInUserInteractivelyAsync(scopes).ConfigureAwait(false);
    }

    public async Task SignOutAsync()
    {
        await MSALClientHelper.SignOutUserAsync().ConfigureAwait(false);
    }

    internal string[] GetScopes()
    {
        var scopes = DownstreamApiHelper.DownstreamApiConfig.ScopesArray;
        return scopes;
    }    
}
