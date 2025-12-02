using System.Diagnostics;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class RegistrationPersonalInfoView : ContentPage
{
    public RegistrationPersonalInfoView(RegistrationPersonalInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}