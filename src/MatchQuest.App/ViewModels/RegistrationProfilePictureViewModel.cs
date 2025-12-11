using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MatchQuest.App.ViewModels
{
    public partial class RegistrationProfilePictureViewModel : ObservableObject
    {
        private readonly GlobalViewModel _global;
        private readonly IUserRepository _userRepository;

        public RegistrationProfilePictureViewModel(GlobalViewModel global, IUserRepository userRepository)
        {
            _global = global;
            _userRepository = userRepository;
        }

        [ObservableProperty]
        private string loginMessage;

        [RelayCommand]
        private async Task Next()
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

            // Navigate to the registered "RegisterBiography" route using Shell.
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
                await Shell.Current.GoToAsync("RegisterBiography");
            }
        }

        public ImageSource ProfileImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(_global.Client?.ProfilePicture))
                    return "default_profile_picture.png";
                byte[] imageBytes = Convert.FromBase64String(_global.Client.ProfilePicture);
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
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
    }
}
