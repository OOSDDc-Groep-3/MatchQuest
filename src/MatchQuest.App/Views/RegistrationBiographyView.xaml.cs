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

    private void Editor_Focused(object sender, FocusEventArgs e)
    {

    }
}