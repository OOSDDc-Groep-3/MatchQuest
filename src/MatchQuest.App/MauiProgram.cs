using MatchQuest.App.ViewModels;
using MatchQuest.App.Views;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace MatchQuest.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Rubik-VariableFont_wght.tff", "RubikRegular");
                fonts.AddFont("Rubik-Italic-VariableFont_wght.tff", "RubikItalic");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IUserService, UserService>();

        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<GlobalViewModel>();

        builder.Services.AddTransient<LoginView>().AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegistrationView>().AddTransient<RegistrationViewModel>();
        builder.Services.AddTransient<RegistrationPersonalInfoView>().AddTransient<RegistrationPersonalInfoViewModel>();
        builder.Services.AddTransient<HomeView>().AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ChatView>().AddTransient<ChatViewModel>();
        //Fixed app launch screensize & lock screensize

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(w =>
            {
                w.OnWindowCreated(window =>
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                    // Fixed size - ensure the window is exactly this size
                    var size = new Windows.Graphics.SizeInt32(1600, 1080);
                    appWindow.Resize(size);

                    // Center the window on the primary work area
                    var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
                    var workArea = displayArea.WorkArea;
                    var centerX = workArea.X + (workArea.Width - size.Width) / 2;
                    var centerY = workArea.Y + (workArea.Height - size.Height) / 2;

                    appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });

                    // Prevent the window from being resized, maximized, or minimized
                    if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    {
                        presenter.IsResizable = false;
                        presenter.IsMaximizable = false;
                        presenter.IsMinimizable = false;
                    }
                });
            });
        });
#endif

        return builder.Build();
    }
}