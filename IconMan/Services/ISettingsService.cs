using IconMan.Models;
using System.Collections.ObjectModel;

namespace IconMan.Services;

/// <summary>
/// Persistent data between sessions; serialized to disk.
/// </summary>
public class Settings
{
    /// <summary>
    /// List of <c>.dll/.ico</c> files where icons will be loaded from.
    /// Sensible defaults are used. Non-existent files are silently ignored.
    /// </summary>
    public ObservableCollection<string> IconSources { get; set; } = [
        @"C:\Windows\System32\imageres.dll",
        @"C:\Windows\System32\shell32.dll",
        @"C:\Windows\System32\ddores.dll",
        @"C:\Windows\System32\mmres.dll",
        @"C:\Windows\System32\netshell.dll",
        @"C:\Windows\System32\networkexplorer.dll",
        @"C:\Windows\System32\setupapi.dll",
        @"C:\Windows\System32\wmploc.dll",
        @"C:\Windows\System32\wpdshext.dll",
        @"C:\Windows\System32\dsuiext.dll",
        @"C:\Windows\System32\comres.dll",
    ];

    /// <summary>
    /// List of icons the user has favorited so they can be easily accessed in
    /// the future.
    /// </summary>
    public ObservableCollection<IconSource> FavoriteIcons { get; set; } = [];

    /// <summary>
    /// Unused; may be implemented in the future to remember the directories a
    /// user has recently set the icon for.
    /// </summary>
    public ObservableCollection<string> RecentDirectories { get; set; } = [];
}

/// <summary>
/// Manages serializing / deserializing settings to and from disk.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Get current settings as loaded in memory. Settings must be explicitly
    /// saved.
    /// </summary>
    Settings Settings { get; }

    /// <summary>
    /// Write settings to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Load settings from disk into <see cref="Settings"/>
    /// </summary>
    void Load();

    /// <summary>
    /// Sets in-memory settings back to deault, writing this change to disk.
    /// </summary>
    void Clear();
}
