using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MatchQuest.App.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly GlobalViewModel _global;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        
        [ObservableProperty]
        private string addGameStatus;

        [ObservableProperty]
        private string saveProfileStatus;

        [ObservableProperty]
        private string removeGameStatus;

        public ObservableCollection<Game> UserGames { get; } = new ObservableCollection<Game>();
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        [ObservableProperty]
        private ObservableCollection<Game> selectedGames = new ObservableCollection<Game>();
        
        [ObservableProperty]
        private Game? selectedGame;

        public UserProfileViewModel(GlobalViewModel global, IUserService userService, IGameService gameService)
        {
            _global = global;
            _userService = userService;
            _gameService = gameService;

            LoadGames();
        }
        
        public string Name
        {
           // Access the global client's name
           
            get => _global.Client.Name;
            set
            {
                _global.Client.Name = value;
                OnPropertyChanged();
            }
        }
        public DateOnly BirthDate
        {
            // Access the global client's birth date
            get => _global.Client?.BirthDate ?? DateOnly.FromDateTime(DateTime.Now);
            set
            {
                    _global.Client.BirthDate = value;
                    OnPropertyChanged();
            }
        }

        public string Region
        {
            // Access the global client's region
            get => _global.Client?.Region ?? string.Empty;
            set
            {
                    _global.Client.Region = value;
                    OnPropertyChanged();
            }
        }

        public string Biography
        {
            // Access the global client's biography
            get => _global.Client?.Biography ?? string.Empty;
            set
            {
                    _global.Client.Biography = value;
                    OnPropertyChanged();
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
            // Validate selection
            if (SelectedGame == null)
            {
                AddGameStatus = "Please select a game to add.";
                await ClearStatusAfterDelay(nameof(AddGameStatus), 4000);

                return;
            }
            
            try
            {
                // Add the game to the database
                _gameService.AddGameToUser(SelectedGame.Id, _global.Client.Id);

                // Clear selection and reload games
                SelectedGame = null;
                LoadGames();

                AddGameStatus = "Game added successfully!";
                // Clear status after delay
                await ClearStatusAfterDelay(nameof(AddGameStatus), 3000);

            }
            catch (Exception ex)
            {
                AddGameStatus = $"Error adding game: {ex.Message}";
                // Clear status after delay
                await ClearStatusAfterDelay(nameof(AddGameStatus), 3000);

            }
        }

        [RelayCommand]
        private async Task RemoveGame(Game? game)
        {
            if (game == null) return;
            
            try
            {
                // Remove the game from the database
                _gameService.RemoveGameFromUser(game.Id, _global.Client.Id);

                // Reload games to update both collections
                LoadGames();
                RemoveGameStatus = "Game removed successfully!";
                // Clear status after delay

                await ClearStatusAfterDelay(nameof(RemoveGameStatus), 1000);

            }
            catch (Exception ex)
            {
                RemoveGameStatus = $"Error removing game: {ex.Message}";
                // Clear status after delay
                await ClearStatusAfterDelay(nameof(RemoveGameStatus), 1000);
                
            }
        }
        
        [RelayCommand]
        private async Task SaveProfile()
        {
            // Update the user profile in the database
            _global.Client = _userService.Update(_global.Client);

            SaveProfileStatus = "Your profile has been updated!";
            // Clear status after delay
            await ClearStatusAfterDelay(nameof(SaveProfileStatus), 4000);
        }
        
        // Helper method to clear status messages after a delay
        private async Task ClearStatusAfterDelay(string propertyName, int milliseconds)
        {
            await Task.Delay(milliseconds);
            
            // Use reflection to set the property to an empty string
            var prop = GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
                prop.SetValue(this, string.Empty);
        }

        // Command to upload a profile photo
        [RelayCommand]
        private async Task UploadProfilePhoto()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a profile picture",
                FileTypes = FilePickerFileType.Images
            });
            
            // If the user canceled the picking
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
            //
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
            var allGames = _gameService.GetAll();

            // Games the user has already added
            var userGames = _gameService.ListByUserId(_global.Client.Id);

            UserGames.Clear();
            foreach (var game in userGames)
                UserGames.Add(game);

            Games.Clear();
            foreach (var game in allGames)
            {
                if (UserGames.All(ug => ug.Id != game.Id))
                {
                    Games.Add(game);
                }
            }
        }
    }
}