using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MsalClientLib;
using System.Reflection;

namespace MauiClient;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddTransient<MainPage>().AddTransient<MainPageViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.appsettings.json");
        builder.Configuration.AddJsonStream(stream);

        Task.Run(PublicClientSingleton.Instance.MSALClientHelper.InitializePublicClientAppAsync).Wait();

        return builder.Build();
    }
}
