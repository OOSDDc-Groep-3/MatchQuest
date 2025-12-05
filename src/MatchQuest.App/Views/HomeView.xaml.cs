using System;
using System.Linq;
using System.Threading.Tasks;
using MatchQuest.App.ViewModels;
using MatchQuest.Core.Models;
using Microsoft.Maui.Controls;

namespace MatchQuest.App.Views;

public partial class HomeView : ContentPage
{
    public HomeView(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Called from XAML CollectionView SelectionChanged="Matches_SelectionChanged"
    private async void Matches_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var user = e?.CurrentSelection?.FirstOrDefault() as User;
        if (user is null)
            return;

        // Clear selection so user can tap the same item again later
        try
        {
            if (sender is CollectionView cv)
                cv.SelectedItem = null;
            else if (MatchesCollection is not null)
                MatchesCollection.SelectedItem = null;
        }
        catch
        {
            // ignore selection clearing errors
        }

        // Prefer invoking the generated RelayCommand on the ViewModel if available
        if (BindingContext is HomeViewModel vm)
        {
            try
            {
                var prop = vm.GetType().GetProperty("OpenChatForCommand");
                if (prop is not null)
                {
                    var cmd = prop.GetValue(vm);
                    if (cmd is not null)
                    {
                        // Try ExecuteAsync(object) first (IAsyncRelayCommand)
                        var execAsync = cmd.GetType().GetMethod("ExecuteAsync", new[] { typeof(object) });
                        if (execAsync is not null)
                        {
                            await (Task)execAsync.Invoke(cmd, new object[] { user })!;
                            return;
                        }

                        // Fallback to Execute(object)
                        var exec = cmd.GetType().GetMethod("Execute", new[] { typeof(object) });
                        if (exec is not null)
                        {
                            exec.Invoke(cmd, new object[] { user });
                            return;
                        }
                    }
                }
            }
            catch
            {
                // if reflection fails, fall back to direct shell navigation below
            }

            // last attempt: dynamic invocation of generated command
            try
            {
                dynamic d = vm;
                if (d.OpenChatForCommand is not null)
                {
                    // Try ExecuteAsync then Execute
                    try { await d.OpenChatForCommand.ExecuteAsync(user); return; } catch { }
                    try { d.OpenChatForCommand.Execute(user); return; } catch { }
                }
            }
            catch
            {
                // ignore
            }
        }

        // Final fallback: navigate directly via Shell to ensure behavior works even if ViewModel wiring is missing
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
            await Shell.Current.GoToAsync("Chat");
        }
    }
}