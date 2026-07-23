using System.Text.RegularExpressions;
using BugBuddy.Models;

namespace BugBuddy.Services;

/// <summary>
/// MSBuild standart hata çıktı formatını parse ederek BuildError listesine dönüştürür.
/// Format: dosya(satır,sütun): severity code: mesaj
/// </summary>
public static partial class ErrorParser
{
    // MSBuild hata formatı regex'i
    // Örnek: /path/Program.cs(15,10): error CS1002: ; expected
    [GeneratedRegex(
        @"^\s*(?<file>[^(]+)\((?<line>\d+),(?<col>\d+)\):\s*(?<severity>error|warning)\s+(?<code>[A-Z]{2,}\d+):\s*(?<message>.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase
    )]
    private static partial Regex MsBuildErrorRegex();

    /// <summary>
    /// Build çıktısını parse ederek hata listesi döner.
    /// </summary>
    public static List<BuildError> Parse(string buildOutput)
    {
        var errors = new List<BuildError>();
        var matches = MsBuildErrorRegex().Matches(buildOutput);

        foreach (Match match in matches)
        {
            var rawMessage = match.Groups["message"].Value.Trim();
            var cleanMessage = Regex.Replace(rawMessage, @"\s+\[.+\]$", "");

            var error = new BuildError(
                FilePath: match.Groups["file"].Value.Trim(),
                Line: int.TryParse(match.Groups["line"].Value, out var line) ? line : 0,
                Column: int.TryParse(match.Groups["col"].Value, out var col) ? col : 0,
                ErrorCode: match.Groups["code"].Value.Trim(),
                Message: cleanMessage,
                Severity: match.Groups["severity"].Value.ToLowerInvariant()
            );

            // Aynı hata kodunu aynı dosya/satırda tekrar ekleme
            if (!errors.Any(e => e.ErrorCode == error.ErrorCode
                              && e.FilePath == error.FilePath
                              && e.Line == error.Line))
            {
                errors.Add(error);
            }
        }

        return errors;
    }
}
