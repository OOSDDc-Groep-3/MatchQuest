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
        private readonly IClientService _clientService;
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;

        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string lastName = "";
        [ObservableProperty] private string region = "";
        [ObservableProperty] private DateTime birthDate = DateTime.Now.AddYears(-18);
        [ObservableProperty] private string loginMessage;

        public RegistrationPersonalInfoViewModel(IClientService clientService, IAuthService authService, GlobalViewModel global)
        {
            _clientService = clientService;
            _auth_service_check(authService);
            _authService = authService;
            _global = global;
        }

        // tiny internal helper to show logs during constructor
        private void _auth_service_check(IAuthService service)
        {
            Debug.WriteLine($"RegistrationPersonalInfoViewModel: ctor - IClientService is {( _clientService == null ? "null" : "present" )}, IAuthService ctor param present? {(service == null ? "no" : "yes")}");
            if (Debugger.IsAttached)
            {
                Debug.WriteLine("RegistrationPersonalInfoViewModel: ctor - debugger attached.");
            }
        }

        [RelayCommand]
        private async Task Confirm()
        {
            Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: Enter.");
            try
            {
                if (_global.Client is null)
                {
                    LoginMessage = "Unexpected error: no registration data found. Please restart registration.";
                    Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: _global.Client is null.");
                    if (Debugger.IsAttached) Debugger.Break();
                    return;
                }

                Debug.WriteLine($"RegistrationPersonalInfoViewModel.Confirm: Global client email='{_global.Client.EmailAddress}', password set?={(string.IsNullOrEmpty(_global.Client.Password) ? "no" : "yes")}");

                if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                {
                    LoginMessage = "Please enter your first and last name.";
                    Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: validation failed - missing name.");
                    if (Debugger.IsAttached) Debugger.Break();
                    return;
                }

                if (BirthDate == default)
                {
                    LoginMessage = "Please enter a valid birth date.";
                    Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: validation failed - invalid birthdate.");
                    return;
                }

                // Populate the global client with personal info
                _global.Client.Name = $"{FirstName.Trim()} {LastName.Trim()}";
                _global.Client.Region = string.IsNullOrWhiteSpace(Region) ? null : Region.Trim();
                _global.Client.BirthDate = BirthDate.Date;
                Debug.WriteLine($"RegistrationPersonalInfoViewModel.Confirm: Populated client Name='{_global.Client.Name}', Region='{_global.Client.Region}', BirthDate='{_global.Client.BirthDate}'");

                // Hash password before saving
                var plainPassword = _global.Client.Password ?? string.Empty;
                Debug.WriteLine($"RegistrationPersonalInfoViewModel.Confirm: Hashing password, length={plainPassword.Length}");
                _global.Client.Password = PasswordHelper.HashPassword(plainPassword);
                Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: Password hashed.");

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: breaking before calling client service.");
                    Debugger.Break();
                }

                // Create in DB
                var created = _clientService.Create(_global.Client);
                if (created is null)
                {
                    LoginMessage = "Failed to create user. Please try again.";
                    Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: Create returned null.");
                    return;
                }

                Debug.WriteLine($"RegistrationPersonalInfoViewModel.Confirm: Create succeeded, assigned id={created.Id}");
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
                    Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: Navigating to Home.");
                    await Shell.Current.GoToAsync("Home");
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Registration error: {ex.Message}";
                Debug.WriteLine($"RegistrationPersonalInfoViewModel.Confirm: Exception: {ex}");
                if (Debugger.IsAttached) Debugger.Break();
            }
            finally
            {
                Debug.WriteLine("RegistrationPersonalInfoViewModel.Confirm: Exit.");
            }
        }
    }
}
