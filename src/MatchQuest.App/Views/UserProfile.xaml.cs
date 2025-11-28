using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class UserProfile : ContentPage
{
    public UserProfile(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
}