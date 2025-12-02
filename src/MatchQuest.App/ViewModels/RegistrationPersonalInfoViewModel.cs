using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using MatchQuest.Core.Helpers;

namespace MatchQuest.App.ViewModels
{
    public partial class RegistrationPersonalInfoViewModel : BaseViewModel
    {
        private readonly IUserService _clientService;
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;

        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string lastName = "";
        [ObservableProperty] private string region = "";
        [ObservableProperty] private DateTime birthDate = DateTime.Now.AddYears(-18);
        [ObservableProperty] private string loginMessage;

        public RegistrationPersonalInfoViewModel(IUserService clientService, IAuthService authService, GlobalViewModel global)
        {
            _clientService = clientService;
            _authService = authService;
            _global = global;
        }

        [RelayCommand]
        private async Task Confirm()
        {
            try
            {
                if (_global.Client is null)
                {
                    LoginMessage = "Unexpected error: no registration data found. Please restart registration.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                {
                    LoginMessage = "Please enter your first and last name.";
                    return;
                }

                if (BirthDate == default)
                {
                    LoginMessage = "Please enter a valid birth date.";
                    return;
                }

                // Populate the global client with personal info
                _global.Client.Name = $"{FirstName.Trim()} {LastName.Trim()}";
                _global.Client.Region = string.IsNullOrWhiteSpace(Region) ? null : Region.Trim();
                _global.Client.BirthDate = BirthDate.Date;

                // Hash password before saving
                var plainPassword = _global.Client.Password ?? string.Empty;
                _global.Client.Password = PasswordHelper.HashPassword(plainPassword);

                // Create in DB
                var created = _clientService.Create(_global.Client);
                if (created is null)
                {
                    LoginMessage = "Failed to create user. Please try again.";
                    return;
                }

                _global.Client = created;

                // Navigate to Home (or other next screen).
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
                    await Shell.Current.GoToAsync("Home");
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Registration error: {ex.Message}";
            }
        }
    }
}
