namespace BugBuddy.Models;

/// <summary>
/// MSBuild çıktısından parse edilen bir build hatasını temsil eder.
/// </summary>
public record BuildError(
    string FilePath,
    int Line,
    int Column,
    string ErrorCode,
    string Message,
    string Severity // "error" veya "warning"
);
