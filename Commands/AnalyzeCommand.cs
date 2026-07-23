using System.CommandLine;
using BugBuddy.Models;
using BugBuddy.Rendering;
using BugBuddy.Services;
using Spectre.Console;

namespace BugBuddy.Commands;

/// <summary>
/// "bugbuddy analyze" komutu: Belirli bir hata kodunu açıklar.
/// </summary>
public static class AnalyzeCommand
{
    public static Command Create()
    {
        var codeArgument = new Argument<string>(
            name: "code",
            description: "The error code to explain (e.g., CS1002, CS0246)"
        );

        var command = new Command("analyze", "Get a friendly explanation for a specific error code")
        {
            codeArgument
        };

        command.SetHandler(async (string code) =>
        {
            ConsoleRenderer.RenderHeader();

            // Hata kodunu normalize et (CS1002 veya cs1002 kabul et)
            code = code.Trim().ToUpperInvariant();

            var settings = AppSettings.Load();

            ErrorExplanation explanation;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync($"Looking up {code}... 🔍", async ctx =>
                {
                    if (!string.IsNullOrWhiteSpace(settings.ApiKey))
                    {
                        // AI ile açıkla
                        var aiService = new AiExplainerService(settings.ApiKey, settings.Model, settings.Provider);
                        var dummyError = new BuildError("unknown", 0, 0, code, $"Error {code}", "error");
                        explanation = await aiService.ExplainAsync(dummyError, settings.Language);
                    }
                    else
                    {
                        // Fallback sözlük
                        var dummyError = new BuildError("unknown", 0, 0, code, $"Error {code}", "error");
                        explanation = BuiltInExplainerService.Explain(dummyError, settings.Language);
                    }

                    ConsoleRenderer.RenderAnalysis(code, explanation, settings.Language);
                });

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                AnsiConsole.MarkupLine("[dim]  💡 Tip: Set your OpenAI API key for smarter explanations:[/]");
                AnsiConsole.MarkupLine("[dim]     bugbuddy config --api-key YOUR_KEY[/]");
                AnsiConsole.WriteLine();
            }

        }, codeArgument);

        return command;
    }
}
