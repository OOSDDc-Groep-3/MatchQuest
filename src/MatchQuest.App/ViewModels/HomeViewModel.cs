using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MatchQuest.App.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly GlobalViewModel _global;
        private readonly IMatchService _matchService;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IReactionService _reactionService;

        [ObservableProperty]
        private bool isToastVisible;

        [ObservableProperty]
        private string toastMessage;

        [ObservableProperty]
        private Color toastBackgroundColor;
        
        // Token to handle resetting the timer if multiple toasts fire quickly
        private CancellationTokenSource _toastCts;
        
        private int CurrentUserId => _global?.Client?.Id ?? 0;

        // Change the Matches collection type
        public ObservableCollection<MatchChatItemViewModel> Matches { get; } = new();

        private List<MatchingScore> MatchPool { get; set; }
        
        [ObservableProperty]
        private MatchingScore? currentMatch;

        [ObservableProperty] private int currentMatchAge = 0;
        
        public bool HasMatch => CurrentMatch != null;
        
        public ImageSource CurrentProfileImage
            => GetProfileImageSource(CurrentMatch?.Matcher);
        
        public ObservableCollection<string> GameTypeOptions { get; } =
            new ObservableCollection<string>(
                new[] { "All" }.Concat(Enum.GetNames(typeof(GameType)))
            );
        
        [ObservableProperty]
        private int selectedGameTypeIndex = 0;
        
        // Selected GameType (nullable = empty option)
        public GameType? SelectedGameType =>
            SelectedGameTypeIndex == 0
                ? null
                : (GameType)(SelectedGameTypeIndex - 1);

        public HomeViewModel(
            GlobalViewModel global, 
            IMatchService matchService, 
            IUserService userService, 
            IChatService chatService, 
            IReactionService reactionService)
        {
            _global = global;
            _matchService = matchService;
            _chatService = chatService;
            _userService = userService;
            _reactionService = reactionService;

            LoadMatches();
            LoadMatchPool();
        }

        private void LoadMatchPool()
        {
            MatchPool = _userService.GetUserMatchPool(_global.Client, SelectedGameType);

            if (MatchPool.Count > 0)
            {
                CurrentMatch = MatchPool.First();
                CurrentMatchAge = CurrentMatch.Matcher.GetAge();
                OnPropertyChanged(nameof(CurrentMatchAge));
            }
            else
            {
                CurrentMatch = null;
            }

            OnPropertyChanged(nameof(HasMatch));
        }
        
        private void LoadMatches()
        {
            if (_global?.Client == null) return;

            var list = _matchService.GetAllMatchesFromUserId(_global.Client.Id);
            Matches.Clear();

            foreach (var u in list)
            {
                var preview = _chatService.GetLastMessagePreview(CurrentUserId, u);
                var matchItem = new MatchChatItemViewModel(u, preview);
                Matches.Add(matchItem);
            }
        }
        
        private void NextMatch()
        {
            if (CurrentMatch == null) return;

            var currentIndex = MatchPool?.IndexOf(CurrentMatch) ?? -1;
            var lastIndex = (MatchPool?.Count ?? 0) - 1;
            
            if (currentIndex >= 0 && currentIndex < lastIndex)
            {
                CurrentMatch = MatchPool[currentIndex + 1];
                OnPropertyChanged(nameof(CurrentMatchAge));
            }
            else
            {
                // We consumed the last match — refresh the pool to fetch new matches.
                LoadMatchPool();
            }
        }
        
        private ImageSource GetProfileImageSource(User? user)
        {
            if (user == null || string.IsNullOrEmpty(user.ProfilePicture))
                return ImageSource.FromFile("default_profile.png");

            try
            {
                var bytes = Convert.FromBase64String(user.ProfilePicture);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                return ImageSource.FromFile("default_profile.png");
            }
        }
        
        private void ShowToast(string message, bool isSuccess)
        {
            // Cancel previous timer if exists
            _toastCts?.Cancel();
            _toastCts = new CancellationTokenSource();

            // Set UI Properties
            ToastMessage = message;
            ToastBackgroundColor = isSuccess ? Colors.SeaGreen : Colors.IndianRed; // Nicer shades of Green/Red
            IsToastVisible = true;

            // Start timer on background thread
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000, _toastCts.Token);
                    // If not cancelled, hide toast on Main Thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        IsToastVisible = false;
                    });
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled because a new toast appeared; do nothing
                }
            });
        }

        private bool CreateReaction(bool isLike)
        {
            Reaction? reaction = null;
            if (CurrentMatch != null)
            {
                reaction = _reactionService.CreateReaction(CurrentUserId, CurrentMatch.Matcher.Id, isLike);
            }
            
            return reaction != null;
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            // Navigate to the registered "Register" route using Shell.
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

        // Called when a match is selected in the UI: set global selected match and navigate to Chat.
        [RelayCommand]
        private async Task OpenChatFor(MatchChatItemViewModel? matchItem)
        {
            if (matchItem?.User is null) return;

            _global.SelectedMatch = matchItem.User;

            // Ensure Shell is available (same pattern used elsewhere)
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
                await Shell.Current.GoToAsync("Chat");
            }
        }

        [RelayCommand]
        private void Like()
        {
            if (CurrentMatch == null)
            {
                return;
            }

            if (!CreateReaction(true))
            {
                ShowToast("Failed to like user.", false);
            }
            else
            {
                ShowToast($"Liked {CurrentMatch.Matcher.Name}!", true);
            }
            
            LoadMatches();
            NextMatch();
        }

        [RelayCommand]
        private void Dislike()
        {
            if (CurrentMatch == null)
            {
                return;
            }
            
            var success = CreateReaction(false);

            if (!success)
            {
                ShowToast("Failed to dislike user.", false);
            }
            else
            {
                ShowToast($"Disliked {CurrentMatch.Matcher.Name}!", false);
            }
            
            NextMatch();
        }
        
        partial void OnCurrentMatchChanged(MatchingScore? value)
        {
            if (value != null)
            {
                currentMatchAge = value.Matcher.GetAge();
            }
            
            OnPropertyChanged(nameof(CurrentMatchAge));
            OnPropertyChanged(nameof(CurrentProfileImage));
            OnPropertyChanged(nameof(HasMatch)); 
        }
        
        partial void OnSelectedGameTypeIndexChanged(int value)
        {
            LoadMatchPool();
        }
    }
}
