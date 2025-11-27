using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.App;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace MatchQuest.App.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly GlobalViewModel _global;

        public HomeViewModel(GlobalViewModel global)
        {
            _global = global;
        }

        [ObservableProperty]
        private string email = "user3@mail.com";

        [ObservableProperty]
        private string password = "user3";

        [ObservableProperty]
        private string loginMessage;

        [RelayCommand]
        private async Task Settings()
        {

            if (Shell.Current is null)
            {
                if (Application.Current?.MainPage is not AppShell)
                {
                    Application.Current!.MainPage = new AppShell();
                    // give the UI a moment to attach the Shell
                    await Task.Yield();
                }
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("Register");
            }
        }
    }
}
