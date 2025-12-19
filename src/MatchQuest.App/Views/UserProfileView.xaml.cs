using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class UserProfileView : ContentPage
{
    public UserProfileView(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
}