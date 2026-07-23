namespace BugBuddy.Models;

/// <summary>
/// AI veya yerleşik sözlük tarafından üretilen hata açıklamasını temsil eder.
/// </summary>
public record ErrorExplanation(
    string FriendlyMessage,
    string Solution,
    string? CodeExample = null
);
