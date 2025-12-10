using System;
using System.Collections.ObjectModel;
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
    private async Task SaveProfile()
    {
        if (_global.Client == null) return;

        // -------------------------
        // Profiel bijwerken
        // -------------------------
        var updated = _userRepository.Update(_global.Client);
        if (updated != null)
            _global.Client = updated;

        // -------------------------
        // Voeg geselecteerde game toe
        // -------------------------
        if (SelectedGame != null)
        {
            try
            {
                _userGameRepository.AddUserGame(_global.Client.Id, SelectedGame.Id);
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Could not add the selected game.",
                    "OK"
                );
            }

            SelectedGame = null;
            LoadGames();

        }

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
    

   
    private void LoadGames()
    {
        // Alle games uit database
        var allGames = _gameRepository.GetAll();

        // Games die de user al heeft
        var userGames = _userGameRepository.GetGamesForUser(_global.Client.Id);

        // Vul UserGames (voor profielweergave)
        UserGames.Clear();
        foreach (var g in userGames)
            UserGames.Add(g);

        // Vul Games (voor toevoegen) alleen met games die gebruiker nog niet heeft
        var userGameIds = userGames.Select(g => g.Id).ToList();
        Games.Clear();
        foreach (var g in allGames.Where(g => !userGameIds.Contains(g.Id)))
            Games.Add(g);
    }


}
