using CommunityToolkit.Mvvm.ComponentModel;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly GlobalViewModel _global;

    public UserProfileViewModel(GlobalViewModel global)
    {
        _global = global;
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

    public DateTime BirthDate
    {
        get => _global.Client?.BirthDate ?? DateTime.Now;
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
}