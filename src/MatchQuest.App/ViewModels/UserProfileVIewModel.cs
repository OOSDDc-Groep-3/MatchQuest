using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly GlobalViewModel _global;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly UserGameRepository _userGameRepository;

        // Collections
        public ObservableCollection<Game> UserGames { get; } = new ObservableCollection<Game>();
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        // Selected game
        [ObservableProperty] private Game? selectedGame;

        public UserProfileViewModel(GlobalViewModel global,
            IUserRepository userRepository,
            IGameRepository gameRepository,
            UserGameRepository userGameRepository)
        {
            _global = global;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _userGameRepository = userGameRepository;

            // Zorg dat games geladen worden als er een client is
            if (_global.Client != null)
            {
                LoadGames();
            }
        }

        #region User Info Properties

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

        public ImageSource ProfileImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(_global.Client?.ProfilePicture))
                    return "carlala.png";

                try
                {
                    var bytes = Convert.FromBase64String(_global.Client.ProfilePicture);
                    return ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                catch
                {
                    return "carlala.png";
                }
            }
        }

        #endregion

        #region Commands

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

            if (_global?.Client == null) return;

            try
            {
                _userGameRepository.AddUserGame(_global.Client.Id, SelectedGame.Id);
                SelectedGame = null;
                LoadGames();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Could not add the selected game.",
                    "OK"
                );
                Debug.WriteLine($"UserProfileViewModel.AddGame: {ex}");
            }
        }

        [RelayCommand]
        private async Task RemoveGame(Game game)
        {
            if (game == null) return;
            if (_global?.Client == null) return;

            try
            {
                _userGameRepository.RemoveUserGame(_global.Client.Id, game.Id);
                LoadGames();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Could not remove the game.",
                    "OK"
                );
                Debug.WriteLine($"UserProfileViewModel.RemoveGame: {ex}");
            }
        }

        [RelayCommand]
        private async Task SaveProfile()
        {
            if (_global.Client == null) return;

            // Update profiel
            var updated = _userRepository.Update(_global.Client);
            if (updated != null)
            {
                _global.Client = updated;
                
                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    "Your profile has been updated!",
                    "OK"
                );
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to update profile.",
                    "OK"
                );
            }
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

        #endregion

        #region Load Games

        public void LoadGames()
        {
            if (_global.Client == null) return;

            var allGames = _gameRepository.GetAll() ?? new List<Game>();
            var userGamesFromDb = _userGameRepository.GetGamesForUser(_global.Client.Id) ??
                                  new System.Collections.Generic.List<Game>();

            // Vul UserGames (voor profielweergave)
            UserGames.Clear();
            foreach (var g in userGamesFromDb)
                UserGames.Add(g);

            // Vul Games (voor toevoegen) alleen met games die gebruiker nog niet heeft
            var userGameIds = userGamesFromDb.Select(g => g.Id).ToList();
            Games.Clear();
            foreach (var g in allGames.Where(g => !userGameIds.Contains(g.Id)))
                Games.Add(g);
        }

        #endregion
    }
}