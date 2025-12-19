using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Services;

namespace MatchQuest.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;

        [ObservableProperty]
        private string _email = "user1@mail.com";

        [ObservableProperty]
        private string _password = "user1";

        [ObservableProperty]
        private string _loginMessage;

        public LoginViewModel(IAuthService authService, GlobalViewModel global)
        {
            _authService = authService; 
            _global = global;
        }

        [RelayCommand]
        private async Task Login()
        {
            // Authenticate client
            var authenticatedClient = _authService.Login(Email, Password);
            if (authenticatedClient != null)
            {
                LoginMessage = $"Welcome {authenticatedClient.Name}!";
                _global.Client = authenticatedClient;

                // Persist logged-in user id so app can restore session
                Preferences.Set("current_user_id", authenticatedClient.Id);

                // Ensure AppShell is attached
                if (Application.Current?.MainPage is not AppShell)
                {
                    Application.Current!.MainPage = new AppShell();
                    await Task.Yield();
                }

                // Navigate to Home only when authentication succeeded
                if (Shell.Current is not null)
                {
                    await Shell.Current.GoToAsync("Home");
                }

                return;
            }

            // Authentication failed — do not navigate
            LoginMessage = "Invalid login credentials.";
        }

        [RelayCommand]
        private async Task Register()
        {
            // Navigate to the registered "Register" route using Shell.
            // If Shell.Current is not available yet, ensure AppShell is attached.
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
                await Shell.Current.GoToAsync("Register");
            }
        }
    }
}
