using MsalClientLib;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MauiClient.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            PlatformConfig.Instance.RedirectUri = PublicClientSingleton.Instance.MSALClientHelper.AzureADConfig.RedirectUriWindows;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public static void Activate()
        {
            var windowHandle = ((MauiWinUIWindow)MauiClient.App.Current.Windows[0].Handler.PlatformView).WindowHandle;
            SetForegroundWindow(windowHandle);
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
