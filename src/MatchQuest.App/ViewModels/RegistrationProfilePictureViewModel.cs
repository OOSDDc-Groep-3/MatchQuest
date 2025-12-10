using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;

namespace MatchQuest.App.ViewModels
{
    public partial class RegistrationProfilePictureViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly IUserService _userService;

        [ObservableProperty] private string newEmail = "";
        [ObservableProperty] private string newPassword = "";
        [ObservableProperty] private string newConfirmPassword = "";

        [ObservableProperty]
        private string loginMessage;

        public RegistrationProfilePictureViewModel(IAuthService authService, GlobalViewModel global, IUserService userService)
        {
            _authService = authService;
            _global = global;
            _userService = userService;
        }

        [RelayCommand]
        private async Task Next()
        {
            // Navigate to the registered "RegisterBiography" route using Shell.
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
                await Shell.Current.GoToAsync("RegisterBiography");
            }
        }
    }
}
