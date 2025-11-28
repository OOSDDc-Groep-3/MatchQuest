using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class HomeView : ContentPage
{
    public HomeView(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    private async void SettingsButton_Clicked(object sender, EventArgs e)
    {
        // Navigeer naar UserProfile pagina
        await Navigation.PushAsync(new UserProfile(new UserProfileViewModel()));
    }
}