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
        private readonly IMatchService _matchService;
        private readonly System.Timers.Timer _pollTimer;
        private readonly ChatRepository _chatRepo;

        // Use the logged-in user's id from GlobalViewModel
        private int CurrentUserId => _global?.Client?.Id ?? 0;

        private int _lastMessageId;

        public int ChatId { get; private set; }

        public ObservableCollection<Message> Messages { get; } = new();

        // Expose matches for the left sidebar (populated from IMatchService + GlobalViewModel)
        public ObservableCollection<User> Matches { get; } = new();

        [ObservableProperty] private string messageText = string.Empty;
        [ObservableProperty] private string partnerName = string.Empty; // UI-bound partner name

        public ChatViewModel(IAuthService authService, GlobalViewModel global, IUserService userService, IMatchService matchService)
        {
            _authService = authService;
            _global = global;
            _userService = userService;
            _matchService = matchService;
            _chatRepo = new ChatRepository();

            // Load matches for the left sidebar
            LoadMatches();

            // Load/create chat and messages from DB for either the selected global match or fallback
            InitializeChatFromDatabase();

            // Poll DB for new messages every 4 seconds (tweak interval as needed)
            _pollTimer = new System.Timers.Timer(4000) { AutoReset = true };
            _pollTimer.Elapsed += (s, e) => PollNewMessages();
            _pollTimer.Start();
        }

        private void LoadMatches()
        {
            Matches.Clear();
            if (_global?.Client == null) return;

            var list = _matchService.GetAll(_global.Client.Id) ?? new System.Collections.Generic.List<User>();
            foreach (var u in list) Matches.Add(u);
        }

        private void InitializeChatFromDatabase()
        {
            // If a match was previously selected in the global VM, prefer that
            if (_global?.SelectedMatch is not null)
            {
                OpenChatForInternal(_global.SelectedMatch);
                return;
            }

            // find a match id that involves the current user (fallback)
            var matchId = CurrentUserId == 0 ? 0 : _chatRepo.GetMatchIdForUser(CurrentUserId);

            if (matchId == 0)
            {
                ChatId = 0;
                PartnerName = string.Empty;
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
            Messages.Clear();
            foreach (var m in msgs)
            {
                m.IsOutbound = (m.SenderId == CurrentUserId);
                Messages.Add(m);
            }

            _lastMessageId = msgs.Any() ? msgs.Max(m => m.Id) : 0;
        }

        private async void PollNewMessages()
        {
            try
            {
                if (ChatId == 0) return;

                var allMsgs = _chatRepo.GetMessagesByChatId(ChatId);
                var newMsgs = allMsgs
                    .Where(m => m.Id > _lastMessageId)
                    .OrderBy(m => m.CreatedAt)
                    .ToList();

                if (newMsgs == null || newMsgs.Count == 0) return;

                foreach (var m in newMsgs)
                    m.IsOutbound = (m.SenderId == CurrentUserId);

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

        // Internal helper used both by Initialize and by OpenMatchCommand
        private void OpenChatForInternal(User user)
        {
            if (user is null || CurrentUserId == 0) return;

            _global.SelectedMatch = user;

            // find existing match row for these two users
            var matchId = _chatRepo.GetMatchIdBetween(CurrentUserId, user.Id);
            if (matchId == 0)
            {
                // no match row found; leave ChatId 0 (optionally create match here if desired)
                ChatId = 0;
                PartnerName = user.Name ?? string.Empty;
                Messages.Clear();
                _lastMessageId = 0;
                return;
            }

            // set partner name and ensure chat exists
            PartnerName = user.Name ?? string.Empty;
            ChatId = _chatRepo.GetOrCreateChatByMatchId(matchId);

            // load messages for this chat
            var msgs = _chatRepo.GetMessagesByChatId(ChatId).OrderBy(m => m.CreatedAt).ToList();
            Messages.Clear();
            foreach (var m in msgs)
            {
                m.IsOutbound = (m.SenderId == CurrentUserId);
                Messages.Add(m);
            }

            _lastMessageId = msgs.Any() ? msgs.Max(m => m.Id) : 0;
        }

        [RelayCommand]
        private async Task OpenMatch(User? user)
        {
            if (user is null) return;

            // run the internal logic on the UI thread to ensure observable updates are safe
            await MainThread.InvokeOnMainThreadAsync(() => OpenChatForInternal(user));
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
                SenderId = CurrentUserId,
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