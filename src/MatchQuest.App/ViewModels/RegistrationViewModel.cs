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

        [ObservableProperty]
        private string newEmail = "";

        [ObservableProperty]
        private string newPassword = "";

        [ObservableProperty]
        private string newConfirmPassword = "";

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
            Debug.WriteLine($"RegistrationViewModel.Next: Enter - Email='{newEmail}', Password set?={(string.IsNullOrEmpty(newPassword) ? "no" : "yes")}");
            try
            {
                // Save initial credentials to global client so the next screen can complete registration
                _global.Client = new Client(0, string.Empty, newEmail?.Trim() ?? string.Empty, newPassword ?? string.Empty);
                Debug.WriteLine("RegistrationViewModel.Next: _global.Client assigned.");

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine("RegistrationViewModel.Next: Debugger is attached - breaking.");
                    Debugger.Break();
                }

                // Navigate to the registered "RegisterPersonalInfo" route using Shell.
                if (Shell.Current is null)
                {
                    Debug.WriteLine("RegistrationViewModel.Next: Shell.Current is null - ensuring AppShell is attached.");
                    if (Application.Current?.MainPage is not AppShell)
                    {
                        Application.Current!.MainPage = new AppShell();
                        await Task.Yield();
                    }
                }

                if (Shell.Current is not null)
                {
                    Debug.WriteLine("RegistrationViewModel.Next: Navigating to RegisterPersonalInfo.");
                    await Shell.Current.GoToAsync("RegisterPersonalInfo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegistrationViewModel.Next: Exception: {ex}");
                LoginMessage = "Unexpected error during navigation.";
            }
            finally
            {
                Debug.WriteLine("RegistrationViewModel.Next: Exit.");
            }
        }
    }
}
