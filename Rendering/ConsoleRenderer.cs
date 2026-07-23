using BugBuddy.Models;
using Spectre.Console;

namespace BugBuddy.Rendering;

/// <summary>
/// Spectre.Console kullanarak güzel, renkli ve dostça terminal çıktısı oluşturur.
/// </summary>
public static class ConsoleRenderer
{
    /// <summary>
    /// BugBuddy başlık banner'ını gösterir.
    /// </summary>
    public static void RenderHeader()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("BugBuddy")
            .Centered()
            .Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[dim]Your friendly build error explainer 🐛✨[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Build başarılı olduğunda kutlama mesajı gösterir.
    /// </summary>
    public static void RenderSuccess()
    {
        var panel = new Panel(
            new Markup("[bold green]Build succeeded! No errors found.[/]\n\n" +
                       "[green]🎉 Great job! Your code is clean and ready to go![/]"))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
            Header = new PanelHeader(" ✅ All Good! ", Justify.Center),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Bulunan hata sayısını gösterir.
    /// </summary>
    public static void RenderErrorSummary(int errorCount, int warningCount)
    {
        var parts = new List<string>();

        if (errorCount > 0)
            parts.Add($"[bold red]{errorCount} error(s)[/]");
        if (warningCount > 0)
            parts.Add($"[bold yellow]{warningCount} warning(s)[/]");

        AnsiConsole.MarkupLine($"  Found {string.Join(" and ", parts)} — let's fix them together! 💪");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Tek bir hata ve açıklamasını güzel format ile gösterir.
    /// </summary>
    /// <summary>
    /// Tek bir hata ve açıklamasını güzel format ile gösterir.
    /// </summary>
    public static void RenderError(BuildError error, ErrorExplanation explanation, int index, string language = "en")
    {
        var isTurkish = language.Equals("tr", StringComparison.OrdinalIgnoreCase);
        var isError = error.Severity.Equals("error", StringComparison.OrdinalIgnoreCase);
        var emoji = isError ? "🔴" : "🟡";
        var color = isError ? "red" : "yellow";
        var fileName = Path.GetFileName(error.FilePath);

        var lineWord = isTurkish ? "Satır" : "Line";
        var whatHappenedLabel = isTurkish ? "💬 Ne oldu:" : "💬 What happened:";
        var howToFixLabel = isTurkish ? "🔧 Nasıl düzeltilir:" : "🔧 How to fix:";
        var exampleLabel = isTurkish ? "💡 Örnek:" : "💡 Example:";
        var originalLabel = isTurkish ? "Orijinal Hata:" : "Original:";

        // Başlık
        var headerText = $"{emoji} {error.ErrorCode} — {fileName} ({lineWord} {error.Line})";

        // İçerik oluştur
        var content = new List<string>
        {
            $"[bold]{whatHappenedLabel}[/]",
            $"   [italic]{Markup.Escape(explanation.FriendlyMessage)}[/]",
            "",
            $"[bold]{howToFixLabel}[/]"
        };

        // Çözüm satırlarını ekle
        foreach (var solutionLine in explanation.Solution.Split('\n'))
        {
            content.Add($"   {Markup.Escape(solutionLine)}");
        }

        // Kod örneği varsa ekle
        if (!string.IsNullOrWhiteSpace(explanation.CodeExample))
        {
            content.Add("");
            content.Add($"[bold]{exampleLabel}[/]");
            content.Add($"[dim]{Markup.Escape(explanation.CodeExample)}[/]");
        }

        // Orijinal hata mesajını küçük puntoda göster
        content.Add("");
        content.Add($"[dim]{originalLabel} {Markup.Escape(error.Message)}[/]");

        var panel = new Panel(string.Join("\n", content))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(isError ? Color.Red : Color.Yellow),
            Header = new PanelHeader($" {headerText} ", Justify.Left),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Kapanış mesajı gösterir.
    /// </summary>
    public static void RenderFooter(bool hasErrors)
    {
        if (hasErrors)
        {
            AnsiConsole.MarkupLine("[dim]  Hope that helps! You've got this! 🚀[/]");
        }

        AnsiConsole.MarkupLine("[dim]  Powered by BugBuddy — github.com/eceguvenn/BugBuddy[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Tek bir hata kodunun açıklamasını gösterir (analyze komutu için).
    /// </summary>
    public static void RenderAnalysis(string errorCode, ErrorExplanation explanation, string language = "en")
    {
        var isTurkish = language.Equals("tr", StringComparison.OrdinalIgnoreCase);
        var whatIsLabel = isTurkish ? $"💬 {Markup.Escape(errorCode)} nedir?" : $"💬 What is {Markup.Escape(errorCode)}?";
        var howToFixLabel = isTurkish ? "🔧 Nasıl düzeltilir:" : "🔧 How to fix it:";
        var exampleLabel = isTurkish ? "💡 Örnek:" : "💡 Example:";

        var content = new List<string>
        {
            $"[bold]{whatIsLabel}[/]",
            $"   [italic]{Markup.Escape(explanation.FriendlyMessage)}[/]",
            "",
            $"[bold]{howToFixLabel}[/]"
        };

        foreach (var line in explanation.Solution.Split('\n'))
        {
            content.Add($"   {Markup.Escape(line)}");
        }

        if (!string.IsNullOrWhiteSpace(explanation.CodeExample))
        {
            content.Add("");
            content.Add($"[bold]{exampleLabel}[/]");
            content.Add($"[dim]{Markup.Escape(explanation.CodeExample)}[/]");
        }

        var panel = new Panel(string.Join("\n", content))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1),
            Header = new PanelHeader($" 🔍 {errorCode} ", Justify.Center),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Hata veya bilgi mesajı gösterir.
    /// </summary>
    public static void RenderMessage(string message, string type = "info")
    {
        var (emoji, color) = type switch
        {
            "error" => ("❌", "red"),
            "warning" => ("⚠️", "yellow"),
            "success" => ("✅", "green"),
            _ => ("ℹ️", "blue")
        };

        AnsiConsole.MarkupLine($"  {emoji} [{color}]{Markup.Escape(message)}[/]");
    }
}
