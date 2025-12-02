using System;
using System.Diagnostics;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels
{
    public partial class GlobalViewModel : BaseViewModel
    {
        private Client _client;
        public Client Client
        {
            get => _client;
            set
            {
                _client = value;
                Debug.WriteLine($"GlobalViewModel.Client: set -> Email='{_client?.EmailAddress}', Id={_client?.Id}");
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Debug.WriteLine("GlobalViewModel.Client: debugger attached on set.");
                }
            }
        }
    }
}
