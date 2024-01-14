using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IconMan.Services;

// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0
[JsonSerializable(typeof(Settings))]
internal partial class SourceGenerationContext : JsonSerializerContext {}

public class JsonSettingsService : ISettingsService
{
    public JsonSettingsService(string? configDir = null)
    {
        ConfigDir = configDir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IconMan");
        Load();
    }

    public string ConfigDir { get; init; }
    public string ConfigFile { get => Path.Combine(ConfigDir, "IconMan.settings.json"); }
    public Settings Settings { get; private set; } = new();

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

    public void Save()
    {
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(ConfigFile, JsonSerializer.Serialize(Settings, typeof(Settings), _options));
    }

    public void Clear()
    {
        this.Settings = new();
        if (File.Exists(ConfigFile))
        {
            File.Delete(ConfigFile);
        }
    }

    private readonly JsonSerializerOptions _options = new()
    {
        TypeInfoResolver = SourceGenerationContext.Default,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };
}
