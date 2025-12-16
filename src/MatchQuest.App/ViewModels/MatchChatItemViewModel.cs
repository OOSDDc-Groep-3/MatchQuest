using CommunityToolkit.Mvvm.ComponentModel;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels
{
    /// <summary>
    /// ViewModel representing a matched user with their last message preview for display in chat lists.
    /// </summary>
    public partial class MatchChatItemViewModel : ObservableObject
    {
        /// <summary>
        /// The matched user
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Preview text of the last message in the chat with this user
        /// </summary>
        [ObservableProperty]
        private string? lastMessagePreview;

        public MatchChatItemViewModel(User user, string? lastMessagePreview = null)
        {
            User = user;
            LastMessagePreview = lastMessagePreview;
        }

        // Expose commonly needed user properties for easier binding
        public string Name => User.Name;
        public string? ProfilePicture => User.ProfilePicture;
        public int Id => User.Id;
    }
}