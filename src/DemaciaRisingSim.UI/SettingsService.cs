using System.IO;
using System.Text.Json;

namespace DemaciaRisingSim.UI;

/// <summary>
/// Optimization settings that are persisted between application sessions.
/// </summary>
public sealed class AppSettings
{
    public bool RequireDurandsWorkshop    { get; set; } = true;
    public bool RequireShrineOfVeiledLady { get; set; } = true;
    public bool RequireQuartermaster      { get; set; } = true;
    public int  MaxBuildingLevel          { get; set; } = 4;
    public int  FoodTargetPerSettlement   { get; set; } = 2;
    public int  LumberTarget              { get; set; } = 296_300;
    public int  StoneTarget               { get; set; } = 343_400;
    public int  MetalTarget               { get; set; } = 143_650;
    public int  PetriciteTarget           { get; set; } = 1_450;
}

/// <summary>
/// Loads and saves <see cref="AppSettings"/> to a JSON file under the user's application-data
/// folder (<c>%APPDATA%\DemaciaRisingSim\settings.json</c> on Windows).
/// All I/O errors are silently swallowed so the application degrades gracefully.
/// </summary>
public static class SettingsService
{
    private static readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DemaciaRisingSim",
        "settings.json");

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Loads persisted settings from disk.  Returns a default <see cref="AppSettings"/>
    /// instance if the file does not exist or cannot be read.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
        }
        catch { /* corrupt or unreadable file — fall through to defaults */ }
        return new AppSettings();
    }

    /// <summary>
    /// Persists <paramref name="settings"/> to disk.  Silently ignores any write errors
    /// (e.g. read-only file system, permissions) so a save failure never crashes the app.
    /// </summary>
    public static void Save(AppSettings settings)
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (dir is not null)
                Directory.CreateDirectory(dir);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(settings, _jsonOptions));
        }
        catch { /* non-critical — best-effort */ }
    }
}
