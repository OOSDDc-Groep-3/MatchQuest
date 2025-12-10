using System.Diagnostics;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class RegistrationGamePreferencesView : ContentPage
{
    public RegistrationGamePreferencesView(RegistrationGamePreferencesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}