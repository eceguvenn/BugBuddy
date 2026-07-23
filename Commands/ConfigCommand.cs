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
        var providerOption = new Option<string?>(
            aliases: new[] { "--provider", "-p" },
            description: "Set AI provider ('openai' or 'gemini')"
        );

        var apiKeyOption = new Option<string?>(
            name: "--api-key",
            description: "Set your AI API key for AI-powered explanations"
        );

        var modelOption = new Option<string?>(
            name: "--model",
            description: "Set the AI model to use (e.g. gpt-4o-mini or gemini-1.5-flash)"
        );

        var langOption = new Option<string?>(
            aliases: new[] { "--lang", "-l" },
            description: "Set language (tr for Turkish, en for English)"
        );

        var showOption = new Option<bool>(
            name: "--show",
            description: "Show current configuration"
        );

        var command = new Command("config", "Configure BugBuddy settings")
        {
            providerOption,
            apiKeyOption,
            modelOption,
            langOption,
            showOption
        };

        command.SetHandler((string? provider, string? apiKey, string? model, string? lang, bool show) =>
        {
            var settings = AppSettings.Load();

            if (show)
            {
                // Mevcut ayarları göster
                ShowConfig(settings);
                return;
            }

            var changed = false;

            if (!string.IsNullOrWhiteSpace(provider))
            {
                var normProv = provider.Trim().ToLowerInvariant();
                if (normProv is "gemini" or "google")
                {
                    settings.Provider = "gemini";
                    if (string.IsNullOrWhiteSpace(model) || settings.Model.StartsWith("gpt-") || settings.Model == "gemini-1.5-flash")
                    {
                        settings.Model = "gemini-flash-latest";
                    }
                    changed = true;
                    ConsoleRenderer.RenderMessage("AI Sağlayıcı: Google Gemini 🌐", "success");
                }
                else if (normProv is "openai")
                {
                    settings.Provider = "openai";
                    if (string.IsNullOrWhiteSpace(model) || settings.Model.StartsWith("gemini-"))
                    {
                        settings.Model = "gpt-4o-mini";
                    }
                    changed = true;
                    ConsoleRenderer.RenderMessage("AI Sağlayıcı: OpenAI 🤖", "success");
                }
                else
                {
                    ConsoleRenderer.RenderMessage("Desteklenmeyen sağlayıcı! (Kullanılabilir: 'gemini' veya 'openai')", "warning");
                }
            }

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

            if (!string.IsNullOrWhiteSpace(lang))
            {
                var normalizedLang = lang.Trim().ToLowerInvariant();
                if (normalizedLang is "tr" or "turkish" or "türkçe")
                {
                    settings.Language = "tr";
                    changed = true;
                    ConsoleRenderer.RenderMessage("Dil Türkçe olarak ayarlandı! 🇹🇷", "success");
                }
                else if (normalizedLang is "en" or "english")
                {
                    settings.Language = "en";
                    changed = true;
                    ConsoleRenderer.RenderMessage("Language set to English! 🇬🇧", "success");
                }
                else
                {
                    ConsoleRenderer.RenderMessage("Desteklenmeyen dil! (Kullanılabilir: 'tr' veya 'en')", "warning");
                }
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
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --provider gemini --api-key YOUR_GEMINI_KEY[/]");
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --provider openai --api-key YOUR_OPENAI_KEY[/]");
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --lang tr[/]");
                AnsiConsole.MarkupLine("[dim]    bugbuddy config --show[/]");
            }

            AnsiConsole.WriteLine();
        }, providerOption, apiKeyOption, modelOption, langOption, showOption);

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

        table.AddRow("Provider", $"[yellow]{settings.Provider.ToUpperInvariant()}[/]");
        table.AddRow("API Key", maskedKey);
        table.AddRow("Model", $"[cyan]{settings.Model}[/]");
        table.AddRow("Language", $"[cyan]{settings.Language}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
    }
}
