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
    public partial class RegistrationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly IUserService _userService;

        [ObservableProperty] private string newEmail = "";
        [ObservableProperty] private string newPassword = "";
        [ObservableProperty] private string newConfirmPassword = "";

        [ObservableProperty]
        private string loginMessage;

        public RegistrationViewModel(IAuthService authService, GlobalViewModel global, IUserService userService)
        {
            _authService = authService;
            _global = global;
            _userService = userService;
        }

        [RelayCommand]
        private async Task Next()
        {
            try
            {
                // Basic validation
                var emailTrimmed = newEmail?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(emailTrimmed))
                {
                    LoginMessage = "Please enter an email address.";
                    return;
                }

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

                // Check if email already exists in the database
                var existing = _userService.Get(emailTrimmed);
                if (existing != null)
                {
                    LoginMessage = "An account with this email already exists.";
                    return;
                }

                // Save initial credentials to global client so the next screen can complete registration
                _global.Client = new User(0, string.Empty, emailTrimmed, newPassword ?? string.Empty);

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
            catch (Exception)
            {
                LoginMessage = "Unexpected error during navigation.";
            }
        }
    }
}
