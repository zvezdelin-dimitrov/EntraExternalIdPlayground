namespace MsalClientLib;

public class PlatformConfig
{
    public static PlatformConfig Instance { get; } = new PlatformConfig();

    public string RedirectUri { get; set; }

    public object ParentWindow { get; set; }

    private PlatformConfig()
    {
    }
}
