using CommunityToolkit.Mvvm.ComponentModel;

namespace MatchQuest.App.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        string title = "";

        public virtual void Load() { }
        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
    }
}
