using System.Diagnostics;

namespace BugBuddy.Services;

/// <summary>
/// dotnet build komutunu çalıştırıp stdout/stderr çıktısını yakalar.
/// </summary>
public static class BuildRunner
{
    /// <summary>
    /// Belirtilen dizinde dotnet build çalıştırır.
    /// </summary>
    /// <param name="projectPath">Proje veya solution yolu (varsayılan: mevcut dizin)</param>
    /// <returns>Build çıktısı (stdout + stderr) ve çıkış kodu</returns>
    public static async Task<(string Output, int ExitCode)> RunBuildAsync(string projectPath = ".")
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Build çıktısını İngilizce'ye zorla (regex uyumluluğu için)
        startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";


        using var process = new Process { StartInfo = startInfo };

        var output = new System.Text.StringBuilder();
        var errorOutput = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                errorOutput.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        // stdout ve stderr'i birleştir (hatalar her iki yerde de olabilir)
        var combinedOutput = output.ToString() + Environment.NewLine + errorOutput.ToString();

        return (combinedOutput, process.ExitCode);
    }
}
