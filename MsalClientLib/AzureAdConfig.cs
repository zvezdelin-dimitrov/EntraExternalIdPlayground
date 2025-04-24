namespace MsalClientLib;

public class AzureADConfig
{
    public string Authority { get; set; }

    public string ClientId { get; set; }

    public string TenantId { get; set; }

    public string RedirectUriWindows { get; set; }

    public string CacheFileNameWindows { get; set; }

    public string CacheDirWindows { get; set; }

    public string AndroidRedirectUri { get; set; }

    public string iOSRedirectUri { get; set; }
}
