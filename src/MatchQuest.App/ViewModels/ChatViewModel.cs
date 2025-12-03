using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers; // <-- Explicitly using System.Timers.Timer
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Models;
using MatchQuest.Core.Interfaces.Services;
using Microsoft.Maui.ApplicationModel;

namespace MatchQuest.App.ViewModels
{
    public partial class ChatViewModel : BaseViewModel, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly System.Timers.Timer _simulateIncomingTimer; // <-- Fully qualify Timer
        private readonly int _currentUserId = 999; // sample local user id (replace with _global.Client.Id when available)

        public int ChatId { get; private set; }

        public ObservableCollection<Message> Messages { get; } = new();

        [ObservableProperty] private string messageText = string.Empty;

        public ChatViewModel(IAuthService authService, GlobalViewModel global)
        {
            _authService = authService;
            _global = global;

            // Create a sample chat and messages
            InitializeSampleChat();

            // Simulate incoming messages every 8 seconds to demonstrate live updates
            _simulateIncomingTimer = new System.Timers.Timer(8000) { AutoReset = true }; // <-- Fully qualify Timer here
            _simulateIncomingTimer.Elapsed += (s, e) => SimulateIncomingMessage();
            _simulateIncomingTimer.Start();
        }

        private void InitializeSampleChat()
        {
            var chat = new Chat
            {
                Id = 1,
                MatchId = 42,
                CreatedAt = DateTime.UtcNow
            };
            ChatId = chat.Id;

            // Create sample messages (two participants: _currentUserId and 123)
            chat.Messages.Add(new Message { Id = 1, ChatId = chat.Id, Sender = 123, MessageText = "Hi, you free to play tonight?", CreatedAt = DateTime.UtcNow.AddMinutes(-12) });
            chat.Messages.Add(new Message { Id = 2, ChatId = chat.Id, Sender = _currentUserId, MessageText = "Yes, what time works for you?", CreatedAt = DateTime.UtcNow.AddMinutes(-10) });
            chat.Messages.Add(new Message { Id = 3, ChatId = chat.Id, Sender = 123, MessageText = "Around 20:00 is good.", CreatedAt = DateTime.UtcNow.AddMinutes(-8) });

            // Populate ObservableCollection (view binds to this)
            foreach (var m in chat.Messages.OrderBy(m => m.CreatedAt))
                Messages.Add(m);
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            var text = MessageText?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // Create message object (in real app you'd persist to DB via service)
            var msg = new Message
            {
                Id = new Random().Next(1000, 100000),
                ChatId = ChatId,
                Sender = _currentUserId, // or _global.Client.Id when logged in
                MessageText = text,
                CreatedAt = DateTime.UtcNow
            };

            // Add to collection on UI thread
            await MainThread.InvokeOnMainThreadAsync(() => Messages.Add(msg));

            MessageText = string.Empty;
        }

        // Simulate incoming message for demo purposes
        private async void SimulateIncomingMessage()
        {
            var incoming = new Message
            {
                Id = new Random().Next(1000, 100000),
                ChatId = ChatId,
                Sender = 123,
                MessageText = "Auto-reply: got it!",
                CreatedAt = DateTime.UtcNow
            };

            await MainThread.InvokeOnMainThreadAsync(() => Messages.Add(incoming));
        }

        public void Dispose()
        {
            _simulateIncomingTimer?.Stop();
            _simulateIncomingTimer?.Dispose();
        }
    }
}