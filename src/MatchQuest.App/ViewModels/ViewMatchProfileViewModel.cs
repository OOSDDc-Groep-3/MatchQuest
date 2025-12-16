using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Data.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;

namespace MatchQuest.App.ViewModels
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class ViewMatchProfileViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly UserGameRepository _userGameRepository;
        
        private User? _user;

        [ObservableProperty]
        private int userId;

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private int age;

        [ObservableProperty]
        private string region = string.Empty;

        [ObservableProperty]
        private string biography = string.Empty;

        [ObservableProperty]
        private ImageSource profileImageSource = ImageSource.FromFile("default_profile.png");

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public ViewMatchProfileViewModel(IUserService userService, UserGameRepository userGameRepository)
        {
            _userService = userService;
            _userGameRepository = userGameRepository;
        }

        partial void OnUserIdChanged(int value)
        {
            LoadUserData(value);
        }

        private void LoadUserData(int userId)
        {
            if (userId == 0) return;

            // Load user from service
            _user = _userService.Get(userId);
            
            if (_user == null) return;

            // Populate properties
            UserName = _user.Name ?? "Unknown";
            Age = _user.GetAge();
            Region = _user.Region ?? "Unknown";
            Biography = _user.Biography ?? "No biography available.";

            // Load profile image
            ProfileImageSource = GetProfileImageSource(_user);

            // Load user's games
            LoadGames(userId);
        }

        private void LoadGames(int userId)
        {
            Games.Clear();
            
            var userGames = _userGameRepository.GetGamesForUser(userId);
            
            foreach (var game in userGames)
            {
                Games.Add(game);
            }
        }

        private ImageSource GetProfileImageSource(User user)
        {
            if (string.IsNullOrEmpty(user.ProfilePicture))
                return ImageSource.FromFile("default_profile.png");

            try
            {
                var bytes = Convert.FromBase64String(user.ProfilePicture);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                return ImageSource.FromFile("default_profile.png");
            }
        }

        [RelayCommand]
        private async Task Back()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("Chat");
            }
        }
    }
}