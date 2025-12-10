using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MatchQuest.App.ViewModels
{
    public partial class RegistrationBiographyViewModel : BaseViewModel
    {
        private readonly GlobalViewModel _global;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        [ObservableProperty]
        private string loginMessage;

        public RegistrationBiographyViewModel(GlobalViewModel global, IUserRepository userRepository)
        {
            _global = global;
            _userRepository = userRepository;
        }

        [RelayCommand]
        private async Task Next()
        {
            // Navigate to the registered "RegisterGamePreferences" route using Shell.
            if (Shell.Current is null)
            {
                if (Application.Current?.MainPage is not AppShell)
                {
                    Application.Current!.MainPage = new AppShell();
                    await Task.Yield();
                }
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("RegisterGamePreferences");
            }
        }
    }
}
