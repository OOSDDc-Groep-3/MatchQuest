using MatchQuest.App.ViewModels;
using MatchQuest.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MatchQuest.App;

public partial class App : Application
{
    public App(LoginViewModel viewModel)
    {
        InitializeComponent();
        MainPage = new LoginView(viewModel);
    }
}

