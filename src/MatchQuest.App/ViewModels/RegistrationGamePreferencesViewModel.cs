using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;

namespace MatchQuest.App.ViewModels
{
    public partial class RegistrationGamePreferencesViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly IUserService _userService;
        private readonly IGameService _gamesService;

        [ObservableProperty]
        private string loginMessage;

        // Collection of all available games (not yet added by user)
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        // Collection of games the user has selected/added
        public ObservableCollection<Game> UserGames { get; } = new ObservableCollection<Game>();

        // Currently selected game from the picker/list
        [ObservableProperty]
        private Game? selectedGame;

        public RegistrationGamePreferencesViewModel(
            IAuthService authService, 
            GlobalViewModel global, 
            IUserService userService,
            IGameService gameService)
        {
            _authService = authService;
            _global = global;
            _userService = userService;
            _gamesService = gameService;

            LoadGames();
        }

        private void LoadGames()
        {
            if (_global?.Client == null) return;

            // Get all games from database
            var allGames = _gamesService.GetAll();

            // Get games the user has already added
            var userGames = _gamesService.ListByUserId(_global.Client.Id);

            // Populate UserGames collection (for display)
            UserGames.Clear();
            foreach (var g in userGames)
                UserGames.Add(g);

            // Populate Games collection with only games the user hasn't added yet
            var userGameIds = userGames.Select(g => g.Id).ToList();
            Games.Clear();
            foreach (var g in allGames.Where(g => !userGameIds.Contains(g.Id)))
                Games.Add(g);
        }

        [RelayCommand]
        private async Task AddGame()
        {
            if (SelectedGame == null)
            {
                LoginMessage = "Please select a game to add.";
                return;
            }

            if (_global?.Client == null)
            {
                LoginMessage = "User profile not found.";
                return;
            }

            try
            {
                // Add the game to the database
                _gamesService.AddGameToUser(SelectedGame.Id, _global.Client.Id);

                // Clear selection and reload games
                SelectedGame = null;
                LoadGames();
                LoginMessage = string.Empty;
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error adding game: {ex.Message}";
                Debug.WriteLine($"RegistrationGamePreferencesViewModel.AddGame: Exception: {ex}");
            }
        }

        [RelayCommand]
        private async Task RemoveGame(Game game)
        {
            if (game == null) return;

            if (_global?.Client == null)
            {
                LoginMessage = "User profile not found.";
                return;
            }

            try
            {
                // Remove the game from the database
                _gamesService.RemoveGameFromUser(_global.Client.Id, game.Id);

                // Reload games to update both collections
                LoadGames();
                LoginMessage = string.Empty;
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error removing game: {ex.Message}";
                Debug.WriteLine($"RegistrationGamePreferencesViewModel.RemoveGame: Exception: {ex}");
            }
        }

        [RelayCommand]
        private async Task Confirm()
        {
            if (_global?.Client == null)
            {
                LoginMessage = "User profile not found.";
                return;
            }

            // Validate that at least one game has been added
            if (UserGames.Count == 0)
            {
                LoginMessage = "Please add at least one game before continuing.";
                return;
            }

            // Navigate to the registered "Home" route using Shell.
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
    }
}
