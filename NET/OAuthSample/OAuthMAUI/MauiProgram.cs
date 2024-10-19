using Microsoft.Extensions.Logging;

using CommunityToolkit.Maui.Core;

using OAuthMAUI.Services.CloudProviders;

namespace OAuthMAUI;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        _ = builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

            // Cloud Providers
        _ = builder.Services.AddSingleton<DropBoxProvider>();
        _ = builder.Services.AddSingleton<GoogleDriveProvider>();
        _ = builder.Services.AddSingleton<OneDriveProvider>();

#if DEBUG
		_ = builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
