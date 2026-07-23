using System.Text.Json;

namespace BugBuddy.Models;

/// <summary>
/// Uygulama ayarlarını yönetir (API key, dil tercihi vb.).
/// Ayarlar ~/.bugbuddy/config.json dosyasında saklanır.
/// </summary>
public class AppSettings
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public string Language { get; set; } = "en";

    // Ayar dosyasının yolu
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".bugbuddy"
    );
    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    /// <summary>
    /// Ayarları dosyadan yükler. Dosya yoksa varsayılan ayarları döner.
    /// </summary>
    public static AppSettings Load()
    {
        if (!File.Exists(ConfigFile))
            return new AppSettings();

        var json = File.ReadAllText(ConfigFile);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    /// <summary>
    /// Ayarları dosyaya kaydeder.
    /// </summary>
    public void Save()
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(ConfigFile, json);
    }
}
