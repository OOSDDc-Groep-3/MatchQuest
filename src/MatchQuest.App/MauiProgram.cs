using MatchQuest.App.ViewModels;
using MatchQuest.App.Views;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Services;
using Microsoft.Extensions.Logging;

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
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IClientService, ClientService>();

        builder.Services.AddSingleton<IClientRepository, ClientRepository>();
        builder.Services.AddSingleton<GlobalViewModel>();

        builder.Services.AddTransient<LoginView>().AddTransient<LoginViewModel>();

        return builder.Build();
    }
}