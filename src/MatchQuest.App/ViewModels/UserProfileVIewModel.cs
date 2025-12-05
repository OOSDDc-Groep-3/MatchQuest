using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly GlobalViewModel _global;
    private readonly IUserRepository _userRepository;

    public UserProfileViewModel(GlobalViewModel global, IUserRepository userRepository)
    {
        _global = global;
        _userRepository = userRepository;
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

    [RelayCommand]
    private void SaveProfile()
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

        // Feedback in debug output
        System.Diagnostics.Debug.WriteLine("✅ SaveProfileCommand uitgevoerd");
    }
}
