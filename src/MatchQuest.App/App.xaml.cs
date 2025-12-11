using MatchQuest.App.ViewModels;
using MatchQuest.App.Views;
using MatchQuest.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;

namespace MatchQuest.App;

public partial class App : Application
{
    public App(LoginViewModel viewModel, GlobalViewModel global, IUserService userService)
    {
        InitializeComponent();

        const string prefKey = "current_user_id";

        // If a user id was saved previously, try to restore the logged-in user
        if (Preferences.ContainsKey(prefKey))
        {
            var id = Preferences.Get(prefKey, 0);
            if (id > 0)
            {
                var restored = userService.Get(id);
                if (restored != null)
                {
                    global.Client = restored;
                    MainPage = new AppShell();
                    return;
                }
            }
        }

        // Fallback to login view if no stored user or restore failed
        MainPage = new LoginView(viewModel);

        MainPage.MinimumHeightRequest = 1080;
        MainPage.MinimumWidthRequest = 1600;
        MainPage.MaximumHeightRequest = 1080;
        MainPage.MaximumWidthRequest = 1600;
        MainPage.Title = "MatchQuest";
    } 
}

