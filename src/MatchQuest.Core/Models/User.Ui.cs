namespace MatchQuest.Core.Models
{
    // UI-only additions to the User model (partial to avoid changing DB mapping)
    public partial class User
    {
        // Backing field so we can notify UI when preview changes.
        private string? _lastMessagePreview;

        // Last message text (not persisted). Populated by viewmodels for display in lists.
        public string? LastMessagePreview
        {
            get => _lastMessagePreview;
            set => _lastMessagePreview = value;
        }
    }
}