using System;
using System.Diagnostics;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels
{
    public partial class GlobalViewModel : BaseViewModel
    {
        private User _client;
        public User Client
        {
            get => _client;
            set
            {
                _client = value;
            }
        }

        // Selected match (the other user) used for navigation into chat/profile
        private User? _selectedMatch;
        public User? SelectedMatch
        {
            get => _selectedMatch;
            set => _selectedMatch = value;
        }
    }
}
