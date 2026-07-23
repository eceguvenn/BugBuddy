using System.CommandLine;
using BugBuddy.Models;
using BugBuddy.Rendering;
using BugBuddy.Services;
using Spectre.Console;

namespace BugBuddy.Commands;

/// <summary>
/// "bugbuddy build" komutu: dotnet build çalıştırır, hataları parse eder ve dostça açıklar.
/// </summary>
public static class BuildCommand
{
    public static Command Create()
    {
        var pathArgument = new Argument<string>(
            name: "path",
            getDefaultValue: () => ".",
            description: "Path to the project or solution file (default: current directory)"
        );

        var command = new Command("build", "Build your project and get friendly error explanations")
        {
            pathArgument
        };

        command.SetHandler(async (string path) =>
        {
            ConsoleRenderer.RenderHeader();

            // Build çalıştır
            string buildOutput;
            int exitCode;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Building your project... 🔨", async ctx =>
                {
                    (buildOutput, exitCode) = await BuildRunner.RunBuildAsync(path);

                    // Hata var mı kontrol et
                    if (exitCode == 0)
                    {
                        ConsoleRenderer.RenderSuccess();
                        ConsoleRenderer.RenderFooter(false);
                        return;
                    }

                    // Hataları parse et
                    var errors = ErrorParser.Parse(buildOutput);

                    if (errors.Count == 0)
                    {
                        // Parse edemediğimiz hatalar
                        ConsoleRenderer.RenderMessage(
                            "Build failed, but I couldn't parse the errors. Here's the raw output:",
                            "warning"
                        );
                        AnsiConsole.WriteLine();
                        AnsiConsole.Write(new Panel(Markup.Escape(buildOutput))
                        {
                            Border = BoxBorder.Rounded,
                            BorderStyle = new Style(Color.Yellow),
                            Header = new PanelHeader(" Raw Build Output ", Justify.Center)
                        });
                        ConsoleRenderer.RenderFooter(true);
                        return;
                    }

                    // Hata/uyarı sayısını göster
                    var errorCount = errors.Count(e => e.Severity == "error");
                    var warningCount = errors.Count(e => e.Severity == "warning");
                    ConsoleRenderer.RenderErrorSummary(errorCount, warningCount);

                    // Ayarları yükle
                    var settings = AppSettings.Load();

                    // AI veya fallback ile açıkla
                    ctx.Status("Analyzing errors... 🧠");

                    for (int i = 0; i < errors.Count; i++)
                    {
                        var error = errors[i];
                        ErrorExplanation explanation;

                        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
                        {
                            var aiService = new AiExplainerService(settings.ApiKey, settings.Model);
                            explanation = await aiService.ExplainAsync(error, settings.Language);
                        }
                        else
                        {
                            explanation = BuiltInExplainerService.Explain(error, settings.Language);
                        }

                        ConsoleRenderer.RenderError(error, explanation, i + 1, settings.Language);
                    }

                    ConsoleRenderer.RenderFooter(true);
                });

        }, pathArgument);

        return command;
    }
}
