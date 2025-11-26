using MatchQuest.App.Views;

namespace MatchQuest.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Login", typeof(LoginView));
        }
    }
}
