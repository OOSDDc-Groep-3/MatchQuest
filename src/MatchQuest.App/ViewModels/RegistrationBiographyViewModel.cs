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
        private readonly IUserRepository _userRepository;

        [ObservableProperty]
        private string loginMessage;

        public RegistrationBiographyViewModel(GlobalViewModel global, IUserRepository userRepository)
        {
            _global = global;
            _userRepository = userRepository;
        }

        // Biography property that binds to the global client
        public string Biography
        {
            get => _global.Client?.Bio ?? string.Empty;
            set
            {
                if (_global.Client != null)
                {
                    _global.Client.Bio = value;
                    OnPropertyChanged();
                }
            }
        }

        [RelayCommand]
        private async Task Next()
        {
            // Save the biography to the database before navigating
            if (_global.Client == null)
            {
                LoginMessage = "User profile not found. Please start registration again.";
                return;
            }

            // Validate that biography is not empty
            if (string.IsNullOrWhiteSpace(_global.Client.Bio))
            {
                LoginMessage = "Please enter a biography before continuing.";
                return;
            }

            try
            {
                // Update the user in the database with the new biography
                var updated = _userRepository.Update(_global.Client);
                if (updated != null)
                {
                    _global.Client = updated;
                }
                else
                {
                    LoginMessage = "Failed to save biography. Please try again.";
                    return;
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error saving biography: {ex.Message}";
                Debug.WriteLine($"RegistrationBiographyViewModel.Next: Exception: {ex}");
                return;
            }

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
