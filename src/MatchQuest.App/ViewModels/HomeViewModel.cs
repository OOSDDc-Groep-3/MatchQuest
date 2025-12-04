using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.App.Views;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MatchQuest.App.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly IMatchService _matchService;

        public ObservableCollection<User> Matches { get; } = new ObservableCollection<User>();

        public HomeViewModel(IAuthService authService, GlobalViewModel global, IMatchService matchService)
        {
            _authService = authService;
            _global = global;
            _matchService = matchService;

            LoadMatches();
        }

        private void LoadMatches()
        {
            if (_global?.Client == null)
                return;

            var list = _matchService.GetAll(_global.Client.Id) ?? new System.Collections.Generic.List<User>();
            Matches.Clear();
            foreach (var u in list) Matches.Add(u);
        }

        [RelayCommand]
        private async Task OpenSettings()
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
                await Shell.Current.GoToAsync("Settings");
            }
        }

        // Called when a match is selected in the UI: set global selected match and navigate to Chat.
        [RelayCommand]
        private async Task OpenChatFor(User? user)
        {
            if (user is null)
                return;

            _global.SelectedMatch = user;

            // Ensure Shell is available (same pattern used elsewhere)
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
                await Shell.Current.GoToAsync("Chat");
            }
        }
    }
}
