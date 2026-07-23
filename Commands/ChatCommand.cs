using System.CommandLine;
using BugBuddy.Models;
using BugBuddy.Rendering;
using BugBuddy.Services;
using Spectre.Console;

namespace BugBuddy.Commands;

/// <summary>
/// "bugbuddy chat" komutu: İnteraktif AI sohbet oturumu başlatır.
/// </summary>
public static class ChatCommand
{
    public static Command Create()
    {
        var command = new Command("chat", "Start an interactive AI pair programming chat session");

        command.SetHandler(async () =>
        {
            ConsoleRenderer.RenderHeader();

            var settings = AppSettings.Load();
            var isTurkish = settings.Language.Equals("tr", StringComparison.OrdinalIgnoreCase);

            var welcomeTitle = isTurkish ? "🤖 BugBuddy Canlı Sohbet Modu" : "🤖 BugBuddy Interactive Chat Mode";
            var exitInstruction = isTurkish 
                ? "Çıkmak için '[bold cyan]çıkış[/]' veya '[bold cyan]exit[/]' yazabilirsiniz."
                : "Type '[bold cyan]exit[/]' or '[bold cyan]quit[/]' to end the session.";

            var panel = new Panel(
                new Markup($"[bold cyan]{welcomeTitle}[/]\n[dim]{exitInstruction}[/]"))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Cyan1),
                Padding = new Padding(2, 1)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            var chatService = new AiChatService(settings.ApiKey, settings.Model, settings.Provider);

            // İnteraktif sohbet döngüsü (REPL)
            while (true)
            {
                var promptText = isTurkish ? "[bold green]Siz >[/]" : "[bold green]You >[/]";
                
                var input = AnsiConsole.Prompt(
                    new TextPrompt<string>(promptText)
                        .PromptStyle("white")
                        .AllowEmpty()
                );

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var trimmed = input.Trim().ToLowerInvariant();
                if (trimmed is "exit" or "quit" or "çıkış" or "q")
                {
                    var exitMsg = isTurkish ? "Görüşmek üzere! Kodlamada başarılar! 🚀" : "Goodbye! Happy coding! 🚀";
                    AnsiConsole.MarkupLine($"\n[dim]  {exitMsg}[/]\n");
                    break;
                }

                string reply = "";
                var waitingMsg = isTurkish ? "Düşünüyor... 🧠" : "Thinking... 🧠";

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("cyan"))
                    .StartAsync(waitingMsg, async ctx =>
                    {
                        reply = await chatService.SendMessageAsync(input, settings.Language);
                    });

                // Yanıtı göster
                var headerTitle = isTurkish ? " 🤖 BugBuddy Yanıtı " : " 🤖 BugBuddy Answer ";
                var replyPanel = new Panel(reply)
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Cyan1),
                    Header = new PanelHeader(headerTitle, Justify.Left),
                    Padding = new Padding(2, 1)
                };

                AnsiConsole.Write(replyPanel);
                AnsiConsole.WriteLine();
            }
        });

        return command;
    }
}
