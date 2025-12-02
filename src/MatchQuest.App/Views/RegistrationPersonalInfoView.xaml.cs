using System.Diagnostics;
using MatchQuest.App.ViewModels;

namespace MatchQuest.App.Views;

public partial class RegistrationPersonalInfoView : ContentPage
{
    public RegistrationPersonalInfoView(RegistrationPersonalInfoViewModel viewModel)
    {
        InitializeComponent();

        Debug.WriteLine("RegistrationPersonalInfoView: ctor - assigning RegistrationPersonalInfoViewModel as BindingContext.");
        BindingContext = viewModel;

        if (Debugger.IsAttached)
        {
            Debug.WriteLine("RegistrationPersonalInfoView: debugger attached - breaking.");
            Debugger.Break();
        }
    }
}