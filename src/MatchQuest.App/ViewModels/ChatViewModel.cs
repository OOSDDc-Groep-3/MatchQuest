using CommunityToolkit.Mvvm.ComponentModel;
using MatchQuest.Core.Interfaces.Services;

namespace MatchQuest.App.ViewModels
{
    public partial class ChatViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;

        public ChatViewModel(IAuthService authService, GlobalViewModel global)
        {
            _authService = authService;
            _global = global;
        }
    }
}