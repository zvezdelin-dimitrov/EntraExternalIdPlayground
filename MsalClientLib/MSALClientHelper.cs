using Microsoft.Identity.Client;
// This is needed only if embedded browser is used on WINDOWS
// TODO: Split the app to remove the dependency if not needed
#if WINDOWS
using Microsoft.Identity.Client.Desktop;
#endif
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.IdentityModel.Abstractions;
using System.Diagnostics;

namespace MsalClientLib;

public class MSALClientHelper
{
    public AzureADConfig AzureADConfig;

    public AuthenticationResult AuthResult { get; private set; }

    public bool IsBrokerInitialized { get; private set; }

    public IPublicClientApplication PublicClientApplication { get; private set; }

    public bool UseEmbedded { get; set; }

    private PublicClientApplicationBuilder PublicClientApplicationBuilder;

    // Token Caching setup - Mac
    public static readonly string KeyChainServiceName = "Contoso.MyProduct";

    public static readonly string KeyChainAccountName = "MSALCache";

    // Token Caching setup - Linux
    public static readonly string LinuxKeyRingSchema = "com.contoso.msaltokencache";

    public static readonly string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
    public static readonly string LinuxKeyRingLabel = "MSAL token cache for Contoso.";
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "Contoso");

    private static readonly string PCANotInitializedExceptionMessage = "The PublicClientApplication needs to be initialized before calling this method. Use InitializePublicClientAppAsync() or InitializePublicClientAppForWAMBrokerAsync() to initialize.";

    public MSALClientHelper(AzureADConfig azureADConfig)
    {
        AzureADConfig = azureADConfig;

        InitializePublicClientApplicationBuilder();
    }

    private void InitializePublicClientApplicationBuilder()
    {
        PublicClientApplicationBuilder = PublicClientApplicationBuilder.Create(AzureADConfig.ClientId)
            .WithAuthority(string.Format(AzureADConfig.Authority, AzureADConfig.TenantId))
            .WithExperimentalFeatures() // this is for upcoming logger
            .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false)    // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging. Set Identity Logging level to Warning which is a middle ground
            .WithClientCapabilities(new string[] { "cp1" })                                     // declare this client app capable of receiving CAE events- https://aka.ms/clientcae
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache");
    }

    public async Task<IAccount> InitializePublicClientAppAsync()
    {
        // Initialize the MSAL library by building a public client application
        PublicClientApplication = PublicClientApplicationBuilder
            .WithRedirectUri(PlatformConfig.Instance.RedirectUri)   // redirect URI is set later in PlatformConfig when the platform has been decided
#if WINDOWS
            .WithWindowsEmbeddedBrowserSupport()
#endif
            .Build();

        await AttachTokenCache();
        return await FetchSignedInUserFromCache().ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes the public client application of MSAL.NET with the required information to correctly authenticate the user using the WAM broker.
    /// </summary>
    /// <returns>An IAccount of an already signed-in user (if available)</returns>
    public async Task<IAccount> InitializePublicClientAppForWAMBrokerAsync()
    {
        // Initialize the MSAL library by building a public client application
        PublicClientApplication = PublicClientApplicationBuilder
            .WithRedirectUri(PlatformConfig.Instance.RedirectUri)   // redirect URI is set later in PlatformConfig when the platform is decided
#if ANDROID || IOS
            .WithBroker()
#elif WINDOWS
            .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
#endif
            .WithParentActivityOrWindow(() => PlatformConfig.Instance.ParentWindow)   // This is required when using the WAM broker and is set later in PlatformConfig when the platform has been decided
            .Build();

        IsBrokerInitialized = true;

        await AttachTokenCache();
        return await FetchSignedInUserFromCache().ConfigureAwait(false);
    }

    /// <summary>
    /// Attaches the token cache to the Public Client app.
    /// </summary>
    /// <returns>IAccount list of already signed-in users (if available)</returns>
    private async Task<IEnumerable<IAccount>> AttachTokenCache()
    {
#if WINDOWS
        // Cache configuration and hook-up to public application. Refer to https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache#configuring-the-token-cache
        var storageProperties = new StorageCreationPropertiesBuilder(AzureADConfig.CacheFileNameWindows, AzureADConfig.CacheDirWindows).Build();

        var msalcachehelper = await MsalCacheHelper.CreateAsync(storageProperties);
        msalcachehelper.RegisterCache(PublicClientApplication.UserTokenCache);

        // If the cache file is being reused, we'd find some already-signed-in accounts
        return await PublicClientApplication.GetAccountsAsync().ConfigureAwait(false);
#else
        return null;
#endif
    }

    /// <summary>
    /// Signs in the user and obtains an Access token for a provided set of scopes
    /// </summary>
    /// <returns>Access Token</returns>
    public async Task<string> SignInUserAndAcquireAccessToken(string[] scopes)
    {
        Exception<NullReferenceException>.ThrowOn(() => PublicClientApplication == null, PCANotInitializedExceptionMessage);

        var existingUser = await FetchSignedInUserFromCache().ConfigureAwait(false);

        try
        {
            // 1. Try to sign-in the previously signed-in account
            if (existingUser != null)
            {
                AuthResult = await PublicClientApplication
                    .AcquireTokenSilent(scopes, existingUser)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                if (IsBrokerInitialized)
                {
                    Console.WriteLine("No accounts found in the cache. Trying Window's default account.");

                    AuthResult = await PublicClientApplication
                        .AcquireTokenSilent(scopes, Microsoft.Identity.Client.PublicClientApplication.OperatingSystemAccount)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
                else
                {
                    AuthResult = await SignInUserInteractivelyAsync(scopes);
                }
            }
        }
        catch (MsalUiRequiredException ex)
        {
            // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenInteractive to acquire a token interactively
            Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

            AuthResult = await PublicClientApplication
                .AcquireTokenInteractive(scopes)
                .WithLoginHint(existingUser?.Username ?? string.Empty)
                .ExecuteAsync()
                .ConfigureAwait(false);
        }
        catch (MsalException msalEx)
        {
            if (msalEx.ErrorCode != "authentication_canceled")
            {
                Debug.WriteLine($"Error Acquiring Token interactively:{Environment.NewLine}{msalEx}");
                throw;
            }
        }

        return AuthResult?.AccessToken;
    }

    /// <summary>
    /// Signs the in user and acquire access token for a provided set of scopes.
    /// </summary>
    public async Task<string> SignInUserAndAcquireAccessToken(string[] scopes, string extraclaims)
    {
        Exception<NullReferenceException>.ThrowOn(() => PublicClientApplication == null, PCANotInitializedExceptionMessage);

        try
        {
            // Send the user to Azure AD for re-authentication as a silent acquisition wont resolve any CAE scenarios like an extra claims request
            AuthResult = await PublicClientApplication.AcquireTokenInteractive(scopes)
                    .WithClaims(extraclaims)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
        }
        catch (MsalException msalEx)
        {
            Debug.WriteLine($"Error Acquiring Token:{Environment.NewLine}{msalEx}");
        }

        return AuthResult.AccessToken;
    }

    /// <summary>
    /// Shows a pattern to sign-in a user interactively in applications that are input constrained and would need to fall-back on device code flow.
    /// </summary>
    /// <param name="scopes">The scopes.</param>
    /// <param name="existingAccount">The existing account.</param>
    /// <returns></returns>
    public async Task<AuthenticationResult> SignInUserInteractivelyAsync(string[] scopes, IAccount existingAccount = null)
    {
        Exception<NullReferenceException>.ThrowOn(() => PublicClientApplication == null, PCANotInitializedExceptionMessage);

        if (PublicClientApplication == null)
            throw new NullReferenceException();

        // If the operating system has UI
        if (PublicClientApplication.IsUserInteractive())
        {
            if (PublicClientSingleton.Instance.UseEmbedded)
            {
                return await PublicClientApplication.AcquireTokenInteractive(scopes)
                    .WithLoginHint(existingAccount?.Username ?? string.Empty)
                    .WithUseEmbeddedWebView(true)
                    .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions();

#if IOS
                // Hide the privacy prompt in iOS
                systemWebViewOptions.iOSHidePrivacyPrompt = true;
#endif
                return await PublicClientApplication.AcquireTokenInteractive(scopes)
                    .WithLoginHint(existingAccount?.Username ?? string.Empty)
                    .WithSystemWebViewOptions(systemWebViewOptions)
                    .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }

        // If the operating system does not have UI (e.g. SSH into Linux), you can fallback to device code, however this
        // flow will not satisfy the "device is managed" CA policy.
        return await PublicClientApplication.AcquireTokenWithDeviceCode(scopes, (dcr) =>
        {
            Console.WriteLine(dcr.Message);
            return Task.CompletedTask;
        }).ExecuteAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the first signed-in user's record from token cache
    /// </summary>
    public async Task SignOutUserAsync()
    {
        var existingUser = await FetchSignedInUserFromCache().ConfigureAwait(false);
        await SignOutUserAsync(existingUser).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes a given user's record from token cache
    /// </summary>
    public async Task SignOutUserAsync(IAccount user)
    {
        if (PublicClientApplication == null) return;

        await PublicClientApplication.RemoveAsync(user).ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches the signed in user from MSAL's token cache (if available).
    /// </summary>
    public async Task<IAccount> FetchSignedInUserFromCache()
    {
        Exception<NullReferenceException>.ThrowOn(() => PublicClientApplication == null, PCANotInitializedExceptionMessage);

        // get accounts from cache
        IEnumerable<IAccount> accounts = await PublicClientApplication.GetAccountsAsync().ConfigureAwait(false);

        // Error corner case: we should always have 0 or 1 accounts, not expecting > 1
        // This is just an example of how to resolve this ambiguity, which can arise if more apps share a token cache.
        // Note that some apps prefer to use a random account from the cache.
        if (accounts.Count() > 1)
        {
            foreach (var acc in accounts)
            {
                await PublicClientApplication.RemoveAsync(acc);
            }

            return null;
        }

        return accounts.SingleOrDefault();
    }
}
