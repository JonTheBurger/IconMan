using IconMan.Models;

namespace IconMan.Services;

/// <summary>
/// Manages the custom icon set for directories.
/// </summary>
public interface IDirectoryIconService
{
    /// <summary>
    /// Checks if a directory has a custom icon set.
    /// </summary>
    /// <param name="directory">An existing directory.</param>
    /// <returns>true when a custom icon has been set in a manner IconMan recognizes.</returns>
    bool HasCustomIcon(string directory);

    /// <summary>
    /// Finds the source of a custom icon. <see cref="HasCustomIcon(string)"/>
    /// should be queried before calling this method.
    /// </summary>
    /// <param name="directory">An existing directory.</param>
    /// <returns>Source of the custom icon; the icon itself must be loaded separately.</returns>
    IconSource GetCustomIcon(string directory);

    /// <summary>
    /// Overwrites a custom icon to display for the provided directory. Any
    /// existing custom icon will be overwritten.
    /// </summary>
    /// <param name="directory">An existing directory.</param>
    /// <param name="icon">Source of the custom icon; the icon itself need not be loaded.</param>
    void SetCustomIcon(string directory, IconSource icon);

    /// <summary>
    /// Deletes any custom icons associated with the provided directory.
    /// </summary>
    /// <param name="directory">An existing directory.</param>
    void ResetCustomIcon(string directory);
}
