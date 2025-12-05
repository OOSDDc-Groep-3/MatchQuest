using System;

namespace MatchQuest.App.ViewModels;

public sealed class MatchItemViewModel
{
    public int MatchId { get; init; }
    public int OtherUserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Avatar { get; init; }
    public string? Preview { get; init; }
    public string? Timestamp { get; init; }
}
