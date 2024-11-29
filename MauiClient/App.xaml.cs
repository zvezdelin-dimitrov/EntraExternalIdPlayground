using MsalClientLib;

namespace MauiClient;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        var existinguser = Task.Run(PublicClientSingleton.Instance.MSALClientHelper.InitializePublicClientAppAsync).Result;
        // TODO: Move to appsettings or set it from the UI
        PublicClientSingleton.Instance.UseEmbedded = true;
        MainPage = new AppShell();
    }
}
