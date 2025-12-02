using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class ChatView : ContentPage
{
    public ChatView(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}