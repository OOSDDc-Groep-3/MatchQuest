using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class ChatView : ContentPage
{
    public ChatView(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}