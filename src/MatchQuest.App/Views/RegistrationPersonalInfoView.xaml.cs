using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class RegistrationPersonalInfoView : ContentPage
{
	public RegistrationPersonalInfoView(RegistrationViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}