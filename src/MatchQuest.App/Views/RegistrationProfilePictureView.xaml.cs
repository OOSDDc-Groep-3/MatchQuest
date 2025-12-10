using System.Diagnostics;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class RegistrationProfilePictureView : ContentPage
{
    public RegistrationProfilePictureView(RegistrationProfilePictureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}