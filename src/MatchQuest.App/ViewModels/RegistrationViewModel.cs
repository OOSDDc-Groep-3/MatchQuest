using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels
{
    public partial class RegistrationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;

        [ObservableProperty] private string newEmail = "";
        [ObservableProperty] private string newPassword = "";
        [ObservableProperty] private string newConfirmPassword = "";

        [ObservableProperty]
        private string loginMessage;

        public RegistrationViewModel(IAuthService authService, GlobalViewModel global)
        {
            _authService = authService;
            _global = global;
        }

        [RelayCommand]
        private async Task Next()
        {
            try
            {
                // Confirm passwords match
                if (newPassword != newConfirmPassword)
                {
                    LoginMessage = "Passwords do not match.";
                    return;
                }

                // Confirm if passwords meet minimum length of 5 characters
                if (newPassword.Length < 5)
                {
                    LoginMessage = "Password must be at least 5 characters long.";
                    return;
                }



                if (newPassword != newConfirmPassword)
                {
                    LoginMessage = "Passwords do not match.";
                    return;
                }

                // Save initial credentials to global client so the next screen can complete registration
                _global.Client = new User(0, string.Empty, newEmail?.Trim() ?? string.Empty, newPassword ?? string.Empty);

                // Navigate to the registered "RegisterPersonalInfo" route using Shell.
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
                    await Shell.Current.GoToAsync("RegisterPersonalInfo");
                }
            }
            catch (Exception ex)
            {
                LoginMessage = "Unexpected error during navigation.";
            }
        }
    }
}
