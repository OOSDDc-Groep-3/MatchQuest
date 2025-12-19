using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Models;

public class Message : Entity
{
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string MessageText { get; set; } = string.Empty;

    // UI-only flag - set by the viewmodel so XAML can show left/right correctly.
    // Not persisted to DB.
    public bool IsOutbound { get; set; }

    // UI-only: profile picture URL for the sender (populated by viewmodels).
    // Not persisted to DB.
    public string? ProfilePicture { get; set; }
}