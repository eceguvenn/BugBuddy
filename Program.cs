using System.CommandLine;
using BugBuddy.Commands;

// Root komutu oluştur
var rootCommand = new RootCommand("BugBuddy — Your friendly build error explainer 🐛✨")
{
    BuildCommand.Create(),
    AnalyzeCommand.Create(),
    ConfigCommand.Create(),
    ChatCommand.Create()
};

// Komut satırı argümanlarını çalıştır
return await rootCommand.InvokeAsync(args);
