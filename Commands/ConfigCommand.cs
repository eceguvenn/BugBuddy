using System.CommandLine;
using BugBuddy.Models;
using BugBuddy.Rendering;
using Spectre.Console;

namespace BugBuddy.Commands;

/// <summary>
/// "bugbuddy config" komutu: API key ve diğer ayarları yönetir.
/// </summary>
public static class ConfigCommand
{
    public static Command Create()
    {
        var apiKeyOption = new Option<string?>(
            name: "--api-key",
            description: "Set your OpenAI API key for AI-powered explanations"
        );

        var modelOption = new Option<string?>(
            name: "--model",
            description: "Set the AI model to use (default: gpt-4o-mini)"
        );

        var showOption = new Option<bool>(
            name: "--show",
            description: "Show current configuration"
        );

        var command = new Command("config", "Configure BugBuddy settings")
        {
            apiKeyOption,
            modelOption,
            showOption
        };

        command.SetHandler((string? apiKey, string? model, bool show) =>
        {
            var settings = AppSettings.Load();

            if (show)
            {
                // Mevcut ayarları göster
                ShowConfig(settings);
                return;
            }

            var changed = false;

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                settings.ApiKey = apiKey;
                changed = true;
                ConsoleRenderer.RenderMessage($"API key set successfully! (ends with ...{apiKey[^4..]})", "success");
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                settings.Model = model;
                changed = true;
                ConsoleRenderer.RenderMessage($"Model set to: {model}", "success");
            }

            if (changed)
            {
                settings.Save();
                AnsiConsole.MarkupLine("[dim]  Settings saved to ~/.bugbuddy/config.json[/]");
            }
            else
            {
                ShowConfig(settings);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]  Usage:[/]");
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --api-key YOUR_OPENAI_KEY[/]");
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --model gpt-4o[/]");
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --show[/]");
            }

            AnsiConsole.WriteLine();
        }, apiKeyOption, modelOption, showOption);

        return command;
    }

    private static void ShowConfig(AppSettings settings)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn(new TableColumn("[bold]Setting[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        var maskedKey = string.IsNullOrWhiteSpace(settings.ApiKey)
            ? "[dim]not set[/]"
            : $"[green]****{settings.ApiKey[^4..]}[/]";

        table.AddRow("API Key", maskedKey);
        table.AddRow("Model", $"[cyan]{settings.Model}[/]");
        table.AddRow("Language", $"[cyan]{settings.Language}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
    }
}
