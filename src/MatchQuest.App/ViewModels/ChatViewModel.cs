using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
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
        private readonly IUserService _userService;
        private readonly IMatchService _matchService;
        private readonly IChatService _chatService;
        private readonly System.Timers.Timer _pollTimer;

        private int CurrentUserId => _global?.Client?.Id ?? 0;
        private int _lastMessageId;

        public int ChatId { get; private set; }
        public ObservableCollection<Message> Messages { get; } = new();
        public ObservableCollection<MatchChatItemViewModel> Matches { get; } = new();

        [ObservableProperty] private string messageText = string.Empty;
        [ObservableProperty] private string partnerName = string.Empty;
        [ObservableProperty] private int partnerUserId;

        private const string DefaultProfilePicture = "showcaseprofile.png";

        public ChatViewModel(
            IAuthService authService, 
            GlobalViewModel global, 
            IUserService userService, 
            IMatchService matchService,
            IChatService chatService)
        {
            _authService = authService;
            _global = global;
            _userService = userService;
            _matchService = matchService;
            _chatService = chatService;

            LoadMatches();
            InitializeChatFromDatabase();

            _pollTimer = new System.Timers.Timer(4000) { AutoReset = true };
            _pollTimer.Elapsed += (s, e) => PollNewMessages();
            _pollTimer.Start();
        }

        private void LoadMatches()
        {
            Matches.Clear();
            if (_global?.Client == null) return;

            var list = _matchService.GetAllMatchesFromUserId(_global.Client.Id);
            foreach (var u in list)
            {
                var preview = _chatService.GetLastMessagePreview(CurrentUserId, u);
                var matchItem = new MatchChatItemViewModel(u, preview);
                Matches.Add(matchItem);
            }
        }

        private void InitializeChatFromDatabase()
        {
            if (_global?.SelectedMatch is not null)
            {
                var preview = _chatService.GetLastMessagePreview(CurrentUserId, _global.SelectedMatch);
                var matchItem = new MatchChatItemViewModel(_global.SelectedMatch, preview);
                OpenChatForInternal(matchItem);
                return;
            }

            var matchId = CurrentUserId == 0 ? 0 : _chatService.GetMatchIdForUser(CurrentUserId);

            if (matchId == 0)
            {
                ChatId = 0;
                PartnerName = string.Empty;
                return;
            }

            var otherUserId = _chatService.GetOtherUserIdForMatch(matchId, CurrentUserId);
            if (otherUserId > 0)
            {
                var other = _userService.Get(otherUserId);
                PartnerName = other?.Name ?? string.Empty;
            }

            ChatId = _chatService.GetOrCreateChatByMatchId(matchId);

            var msgs = _chatService.GetMessagesByChatId(ChatId).OrderBy(m => m.CreatedAt).ToList();
            Messages.Clear();
            foreach (var m in msgs)
            {
                m.IsOutbound = (m.SenderId == CurrentUserId);

                if (m.IsOutbound)
                {
                    m.ProfilePicture = _global?.Client?.ProfilePicture ?? DefaultProfilePicture;
                }
                else
                {
                    var sender = _userService.Get(m.SenderId);
                    m.ProfilePicture = sender?.ProfilePicture ?? DefaultProfilePicture;
                }

                Messages.Add(m);
            }

            _lastMessageId = msgs.Any() ? msgs.Max(m => m.Id) : 0;
        }

        private async void PollNewMessages()
        {
            try
            {
                if (ChatId == 0) return;

                var allMsgs = _chatService.GetMessagesByChatId(ChatId);
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
                    {
                        if (m.IsOutbound)
                        {
                            m.ProfilePicture = _global?.Client?.ProfilePicture ?? DefaultProfilePicture;
                        }
                        else
                        {
                            var sender = _userService.Get(m.SenderId);
                            m.ProfilePicture = sender?.ProfilePicture ?? DefaultProfilePicture;
                        }

                        Messages.Add(m);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PollNewMessages error: {ex}");
            }
        }

        private void OpenChatForInternal(MatchChatItemViewModel matchItem)
        {
            if (matchItem?.User is null || CurrentUserId == 0) return;
            
            var user = matchItem.User;
            _global.SelectedMatch = user;
            PartnerUserId = user.Id;

            var matchId = _chatService.GetMatchIdBetween(CurrentUserId, user.Id);
            if (matchId == 0)
            {
                ChatId = 0;
                PartnerName = user.Name ?? string.Empty;
                Messages.Clear();
                _lastMessageId = 0;
                return;
            }

            PartnerName = user.Name ?? string.Empty;
            ChatId = _chatService.GetOrCreateChatByMatchId(matchId);

            var msgs = _chatService.GetMessagesByChatId(ChatId).OrderBy(m => m.CreatedAt).ToList();
            Messages.Clear();
            foreach (var m in msgs)
            {
                m.IsOutbound = (m.SenderId == CurrentUserId);

                if (m.IsOutbound)
                {
                    m.ProfilePicture = _global?.Client?.ProfilePicture ?? DefaultProfilePicture;
                }
                else
                {
                    var sender = _userService.Get(m.SenderId);
                    m.ProfilePicture = sender?.ProfilePicture ?? DefaultProfilePicture;
                }

                Messages.Add(m);
            }

            _lastMessageId = msgs.Any() ? msgs.Max(m => m.Id) : 0;
        }

        [RelayCommand]
        private async Task OpenMatch(MatchChatItemViewModel? matchItem)
        {
            if (matchItem is null) return;
            await MainThread.InvokeOnMainThreadAsync(() => OpenChatForInternal(matchItem));
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
                IsOutbound = true,
                ProfilePicture = _global?.Client?.ProfilePicture ?? DefaultProfilePicture
            };

            msg.Id = _chatService.SendMessage(msg);

            if (msg.Id > _lastMessageId) _lastMessageId = msg.Id;

            await MainThread.InvokeOnMainThreadAsync(() => Messages.Add(msg));

            MessageText = string.Empty;
        }

        [RelayCommand]
        private async Task Back()
        {
            if (Shell.Current is null)
            {
                if (Application.Current?.MainPage is not AppShell)
                {
                    Application.Current!.MainPage = new AppShell();
                    await Task.Yield();
                }
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("Home");
            }
        }

        [RelayCommand]
        private async Task ViewPartnerProfile()
        {
            if (PartnerUserId == 0) return;

            if (Shell.Current is null)
            {
                if (Application.Current?.MainPage is not AppShell)
                {
                    Application.Current!.MainPage = new AppShell();
                    await Task.Yield();
                }
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync($"ViewMatchProfile?userId={PartnerUserId}");
            }
        }

        public void Dispose()
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();
        }
    }
}