using MatchQuest.App.Views;

namespace MatchQuest.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Login", typeof(LoginView));
            Routing.RegisterRoute("Register", typeof(RegistrationView));
            Routing.RegisterRoute("RegisterPersonalInfo", typeof(RegistrationPersonalInfoView));
            Routing.RegisterRoute("HomeView", typeof(HomeView));
            Routing.RegisterRoute("ChatView", typeof(ChatView));
        }
    }
}
