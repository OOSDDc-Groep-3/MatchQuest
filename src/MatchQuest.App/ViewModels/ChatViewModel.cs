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

            // Poll for new messages every 4 seconds
            _pollTimer = new System.Timers.Timer(4000) { AutoReset = true };
            _pollTimer.Elapsed += (s, e) => PollNewMessages();
            _pollTimer.Start();
        }

        // Load all matches for the current user with message previews
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

        // Initialize chat from database (either selected match or first available match)
        private void InitializeChatFromDatabase()
        {
            // If a match was selected from another view, open that chat
            if (_global?.SelectedMatch is not null)
            {
                var preview = _chatService.GetLastMessagePreview(CurrentUserId, _global.SelectedMatch);
                var matchItem = new MatchChatItemViewModel(_global.SelectedMatch, preview);
                OpenChat(matchItem);
                return;
            }

            // Otherwise, load the first match for the current user
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

            // Load all messages and set profile pictures
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

        // Poll for new messages and add them to the UI
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

                // Mark messages as outbound or inbound
                foreach (var m in newMsgs)
                    m.IsOutbound = (m.SenderId == CurrentUserId);

                _lastMessageId = newMsgs.Max(m => m.Id);

                // Update UI on main thread
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

        // Open a chat with a specific match
        private void OpenChat(MatchChatItemViewModel matchItem)
        {
            if (matchItem?.User is null || CurrentUserId == 0) return;
            
            var user = matchItem.User;
            _global.SelectedMatch = user;
            PartnerUserId = user.Id;

            var matchId = _chatService.GetMatchIdBetween(CurrentUserId, user.Id);
            if (matchId == 0)
            {
                // No match found, display empty chat
                ChatId = 0;
                PartnerName = user.Name ?? string.Empty;
                Messages.Clear();
                _lastMessageId = 0;
                return;
            }

            PartnerName = user.Name ?? string.Empty;
            ChatId = _chatService.GetOrCreateChatByMatchId(matchId);

            // Load all messages and set profile pictures
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

        // Command to open a chat when a match is selected from the list
        [RelayCommand]
        private async Task OpenMatch(MatchChatItemViewModel? matchItem)
        {
            if (matchItem is null) return;
            await MainThread.InvokeOnMainThreadAsync(() => OpenChat(matchItem));
        }

        // Command to send a message
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

        // Command to navigate back to the home page
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

        // Command to view the partner's profile
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

        [RelayCommand]
        private async Task OpenSettings()
        {
            // Navigate to the registered "UserProfile" route using Shell.
            // If Shell.Current is not available yet, ensure AppShell is attached.
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
                await Shell.Current.GoToAsync("UserProfile");
            }
        }

        // Clean up resources when view model is disposed
        public void Dispose()
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();
        }
    }
}