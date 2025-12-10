using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly GlobalViewModel _global;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;

    
    public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

    [ObservableProperty]
    private Game selectedGame;

    public UserProfileViewModel(GlobalViewModel global, IUserRepository userRepository, IGameRepository gameRepository)
    {
        _global = global;
        _userRepository = userRepository;
        _gameRepository = gameRepository;

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

       

        if (_global.Client.Id == 0)
        {
            var created = _userRepository.Add(_global.Client);
            if (created != null)
                _global.Client = created;
        }
        else
        {
            var updated = _userRepository.Update(_global.Client);
            if (updated != null)
                _global.Client = updated;
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
        var list = _gameRepository.GetAll();
        Games.Clear();

        foreach (var g in list)
            Games.Add(g);

        
       
    }
}
