using MsalClientLib;

namespace MauiClient;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        var existinguser = Task.Run(PublicClientSingleton.Instance.MSALClientHelper.InitializePublicClientAppAsync).Result;
        MainPage = new AppShell();
    }
}
