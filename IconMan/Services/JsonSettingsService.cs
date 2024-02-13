using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IconMan.Services;

/// <summary>
/// When using .NET AOT, types for JSON serialization must be explicitly called out. See:
/// <a href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0"/>
/// </summary>
[JsonSerializable(typeof(Settings))]
internal partial class SourceGenerationContext : JsonSerializerContext {}

/// <summary>
/// Use a JSON file to store persistent settings.
/// </summary>
public class JsonSettingsService : ISettingsService
{
    /// <summary>
    /// Initialize the service, optionally overriding the config directory
    /// (e.g. for testing). Loads settings from an existing config, otherwise
    /// newly creates the config file and directory.
    /// </summary>
    /// <param name="configDir">
    /// Directory where <c>IconMan.settings.json</c> will be created. If not
    /// set, a sensible default will be used.
    /// </param>
    public JsonSettingsService(string? configDir = null)
    {
        ConfigDir = configDir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IconMan");
        Load();
    }

    /// <summary>
    /// Directory containing config file.
    /// </summary>
    public string ConfigDir { get; init; }

    /// <summary>
    /// Path to the config file.
    /// </summary>
    public string ConfigFile { get => Path.Combine(ConfigDir, "IconMan.settings.json"); }

    /// <inheritdoc/>
    public Settings Settings { get; private set; } = new();

    /// <inheritdoc/>
    public void Load()
    {
        if (File.Exists(ConfigFile))
        {
            string text = File.ReadAllText(ConfigFile);
            Settings? settings = JsonSerializer.Deserialize(text, typeof(Settings), _options) as Settings;
            if (settings != null)
            {
                Settings = settings;
            }
        }
    }

    /// <inheritdoc/>
    public void Save()
    {
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(ConfigFile, JsonSerializer.Serialize(Settings, typeof(Settings), _options));
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Settings = new();
        if (File.Exists(ConfigFile))
        {
            File.Delete(ConfigFile);
        }
    }

    /// <summary>
    /// Use settings for petty JSON, as users may wish to hand-edit the file.
    /// </summary>
    private readonly JsonSerializerOptions _options = new()
    {
        // Used for AOT
        TypeInfoResolver = SourceGenerationContext.Default,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };
}
