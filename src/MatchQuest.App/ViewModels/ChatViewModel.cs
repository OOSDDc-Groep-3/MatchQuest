using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Models;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Data.Repositories;
using Microsoft.Maui.ApplicationModel;

namespace MatchQuest.App.ViewModels
{
    public partial class ChatViewModel : BaseViewModel, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly IUserService _userService;
        private readonly System.Timers.Timer _pollTimer;
        private readonly ChatRepository _chatRepo;

        // Use the logged-in user's id from GlobalViewModel
        private int CurrentUserId => _global?.Client?.Id ?? 0;

        private int _lastMessageId;

        public int ChatId { get; private set; }

        public ObservableCollection<Message> Messages { get; } = new();

        [ObservableProperty] private string messageText = string.Empty;
        [ObservableProperty] private string partnerName = string.Empty; // UI-bound partner name

        public ChatViewModel(IAuthService authService, GlobalViewModel global, IUserService userService)
        {
            _authService = authService;
            _global = global;
            _userService = userService;
            _chatRepo = new ChatRepository();

            // Load/create chat and messages from DB
            InitializeChatFromDatabase();

            // Poll DB for new messages every 4 seconds (tweak interval as needed)
            _pollTimer = new System.Timers.Timer(4000) { AutoReset = true };
            _pollTimer.Elapsed += (s, e) => PollNewMessages();
            _pollTimer.Start();
        }

        private void InitializeChatFromDatabase()
        {
            // find a match id that involves the current user
            var matchId = CurrentUserId == 0 ? 0 : _chatRepo.GetMatchIdForUser(CurrentUserId);

            if (matchId == 0)
            {
                ChatId = 0;
                return;
            }

            // set partner name by resolving other participant from matches and users table
            var otherUserId = _chatRepo.GetOtherUserIdForMatch(matchId, CurrentUserId);
            if (otherUserId > 0)
            {
                var other = _userService.Get(otherUserId);
                PartnerName = other?.Name ?? string.Empty;
            }

            // Ensure a chat exists for this match and get its id
            ChatId = _chatRepo.GetOrCreateChatByMatchId(matchId);

            // Load existing messages for this chat and populate ObservableCollection
            var msgs = _chatRepo.GetMessagesByChatId(ChatId).OrderBy(m => m.CreatedAt).ToList();
            foreach (var m in msgs)
            {
                m.IsOutbound = (m.Sender == CurrentUserId);
                Messages.Add(m);
            }

            _lastMessageId = msgs.Any() ? msgs.Max(m => m.Id) : 0;
        }

        private async void PollNewMessages()
        {
            try
            {
                if (ChatId == 0) return;

                var newMsgs = _chatRepo.GetMessagesAfter(ChatId, _lastMessageId);
                if (newMsgs == null || newMsgs.Count == 0) return;

                foreach (var m in newMsgs)
                    m.IsOutbound = (m.Sender == CurrentUserId);

                _lastMessageId = newMsgs.Max(m => m.Id);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var m in newMsgs.OrderBy(m => m.CreatedAt))
                        Messages.Add(m);
                });
            }
            catch (Exception ex)
            {
                // optional: log exception for diagnostics
                System.Diagnostics.Debug.WriteLine($"PollNewMessages error: {ex}");
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            var text = MessageText?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (CurrentUserId == 0) return;

            var msg = new Message
            {
                ChatId = ChatId,
                Sender = CurrentUserId,
                MessageText = text,
                CreatedAt = DateTime.UtcNow,
                IsOutbound = true
            };

            // persist to DB and get id
            msg.Id = _chatRepo.InsertMessage(msg);

            // update last message id so poller won't refetch it
            if (msg.Id > _lastMessageId) _lastMessageId = msg.Id;

            // Add to collection on UI thread
            await MainThread.InvokeOnMainThreadAsync(() => Messages.Add(msg));

            MessageText = string.Empty;
        }

        public void Dispose()
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();
            _chatRepo?.Dispose();
        }
    }
}