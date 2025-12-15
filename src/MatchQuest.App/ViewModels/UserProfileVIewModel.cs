using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;

namespace MatchQuest.App.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly GlobalViewModel _global;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly UserGameRepository _userGameRepository;
        public ObservableCollection<Game> UserGames { get; } = new ObservableCollection<Game>();
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        [ObservableProperty]
        private ObservableCollection<Game> selectedGames = new ObservableCollection<Game>();
        
        [ObservableProperty]
        private Game selectedGame;

        public UserProfileViewModel(GlobalViewModel global,
            IUserRepository userRepository,
            IGameRepository gameRepository,
            UserGameRepository userGameRepository)
        {
            _global = global;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _userGameRepository = userGameRepository;

            LoadGames();
        }
  
        public string Name
        {
            get => _global.Client?.Name ?? string.Empty;
            set
            {
                if (_global.Client != null)
                {
                    _global.Client.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateOnly BirthDate
        {
            get => _global.Client?.BirthDate ?? DateOnly.FromDateTime(DateTime.Now);
            set
            {
                if (_global.Client != null)
                {
                    _global.Client.BirthDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Region
        {
            get => _global.Client?.Region ?? string.Empty;
            set
            {
                if (_global.Client != null)
                {
                    _global.Client.Region = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Biography
        {
            get => _global.Client?.Biography ?? string.Empty;
            set
            {
                if (_global.Client != null)
                {
                    _global.Client.Biography = value;
                    OnPropertyChanged();
                }
            }
        }

        public ImageSource ProfileImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(_global.Client?.ProfilePicture))
                    return "dotnet_bot.png";

                byte[] imageBytes = Convert.FromBase64String(_global.Client.ProfilePicture);
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
        }

        [RelayCommand]
        private void SelectGame(Game game)
        {
            SelectedGame = game;
        }

        [RelayCommand]
        private async Task AddGame()
        {
            if (SelectedGame == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Please select a game to add.",
                    "OK"
                );
                return;
            }

            if (_global?.Client == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "User profile not found.",
                    "OK"
                );
                return;
            }

            try
            {
                // Add the game to the database
                _userGameRepository.AddUserGame(_global.Client.Id, SelectedGame.Id);

                // Clear selection and reload games
                SelectedGame = null;
                LoadGames();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error adding game: {ex.Message}",
                    "OK"
                );
                Debug.WriteLine($"UserProfileViewModel.AddGame: Exception: {ex}");
            }
        }

        [RelayCommand]
        private async Task RemoveGame(Game game)
        {
            if (game == null) return;

            if (_global?.Client == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "User profile not found.",
                    "OK"
                );
                return;
            }

            try
            {
                // Remove the game from the database
                _userGameRepository.RemoveUserGame(_global.Client.Id, game.Id);

                // Reload games to update both collections
                LoadGames();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error removing game: {ex.Message}",
                    "OK"
                );
                Debug.WriteLine($"UserProfileViewModel.RemoveGame: Exception: {ex}");
            }
        }

        [RelayCommand]
        private async Task SaveProfile()
        {
            if (_global.Client == null) return;

            // Validate that at least one game has been added
            if (UserGames.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Please add at least one game before saving.",
                    "OK"
                );
                return;
            }

            // Update profile
            var updated = _userRepository.Update(_global.Client);
            if (updated != null)
                _global.Client = updated;

            await Application.Current.MainPage.DisplayAlert(
                "Success",
                "Your profile has been updated!",
                "OK"
            );
        }

        [RelayCommand]
        private async Task UploadProfilePhoto()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a profile picture",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null && _global.Client != null)
            {
                var base64 = FileHelper.ImageToBase64(result.FullPath);
                _global.Client.ProfilePicture = base64;

                OnPropertyChanged(nameof(ProfileImageSource));
            }
        }

        [RelayCommand]
        private async Task Back()
        {
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
        
        private void LoadGames()
        {
            if (_global?.Client == null) return;

            // All games from database
            var allGames = _gameRepository.GetAll();

            // Games the user has already added
            var userGames = _userGameRepository.GetGamesForUser(_global.Client.Id);

            // Fill UserGames (for profile display)
            UserGames.Clear();
            foreach (var g in userGames)
                UserGames.Add(g);

            // Fill Games (for adding) only with games the user hasn't added yet
            var userGameIds = userGames.Select(g => g.Id).ToList();
            Games.Clear();
            foreach (var g in allGames.Where(g => !userGameIds.Contains(g.Id)))
                Games.Add(g);
        }
    }
}