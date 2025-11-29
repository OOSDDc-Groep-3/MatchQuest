using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.App.Views;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace MatchQuest.App.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [RelayCommand]
        private async Task OpenSettings()
        {
            // Ensure Shell is available. Prefer opening a Window with AppShell instead of Application.MainPage (obsolete).
            if (Shell.Current is null)
            {
                if (Application.Current is not null)
                {
                    if (Application.Current.Windows.Count == 0)
                    {
                        Application.Current.OpenWindow(new Window(new AppShell()));
                        await Task.Yield();
                    }
                    else
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                        await Task.Yield();
                    }
                }
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("Settings");
            }
        }

        [RelayCommand]
        private async Task OpenChat()
        {
            // Ensure Shell is available before navigation.
            if (Shell.Current is null)
            {
                if (Application.Current is not null)
                {
                    if (Application.Current.Windows.Count == 0)
                    {
                        Application.Current.OpenWindow(new Window(new AppShell()));
                        await Task.Yield();
                    }
                    else
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                        await Task.Yield();
                    }
                }
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(nameof(ChatView));
            }
        }
    }
}
