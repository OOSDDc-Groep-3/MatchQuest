using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly GlobalViewModel _global;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly UserGameRepository _userGameRepository;
    
    public ObservableCollection<Game> UserGames { get; } = new ObservableCollection<Game>();
    public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

    [ObservableProperty]
    private Game? selectedGame;

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
                return "dotnet_bot.png";

            byte[] imageBytes = Convert.FromBase64String(_global.Client.ProfilePicture);
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
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

        // Update user profile in database
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

    private void LoadGames()
    {
        if (_global?.Client == null) return;

        // Get all games from database
        var allGames = _gameRepository.GetAll();

        // Get games the user already has
        var userGames = _userGameRepository.GetGamesForUser(_global.Client.Id);

        // Populate UserGames collection
        UserGames.Clear();
        foreach (var g in userGames)
            UserGames.Add(g);

        // Populate Games collection with only games user doesn't have yet
        var userGameIds = userGames.Select(g => g.Id).ToList();
        Games.Clear();
        foreach (var g in allGames.Where(g => !userGameIds.Contains(g.Id)))
            Games.Add(g);
    }
}
