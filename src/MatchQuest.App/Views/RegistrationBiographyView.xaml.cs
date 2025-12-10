using System.Diagnostics;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class RegistrationBiographyView : ContentPage
{
    public RegistrationBiographyView(RegistrationBiographyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}