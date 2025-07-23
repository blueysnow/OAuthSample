using Microsoft.Extensions.Logging;

using CommunityToolkit.Maui.Core;

using OAuthMAUI.Services.CloudProviders;
using Microsoft.Maui.LifecycleEvents;

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

#if WINDOWS
        _ = builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows =>
                {
                    windows.OnWindowCreated(windows =>
                    {
                        var manager = WinUIEx.WindowManager.Get(windows);
                        manager.PersistenceId = "MainWindowPersistenceId";
                        if (WinUIEx.WebAuthenticator.CheckOAuthRedirectionActivation())
                            return;

                    });
                });
        });
#endif

#if DEBUG
        _ = builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
