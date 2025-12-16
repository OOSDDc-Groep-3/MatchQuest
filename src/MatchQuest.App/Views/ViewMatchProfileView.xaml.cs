namespace MatchQuest.App.Views
{
    public partial class ViewMatchProfileView : ContentPage
    {
        public ViewMatchProfileView(ViewModels.ViewMatchProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}