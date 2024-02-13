using Avalonia.Media.Imaging;
namespace IconMan.Models;

/// <summary>
/// Represents a "desktop.ini" entry of the form:
/// <code>
/// "IconResource=C:\Windows\System32\imageres.dll,221"
/// </code>
/// </summary>
/// <remarks>
/// On Windows, an icon is loaded by offsetting into a <c>.dll</c> by index,
/// starting at 0. Files can be queried for the number of icons they contain,
/// but icon indices can change across application versions. Some files provide
/// stable identifiers for icons at negative numbers, but we don't support
/// these.
/// </remarks>
public record IconSource
{
    /// <summary>
    /// <c>.dll/.ico/.exe</c> containing the icon bitmap. An empty string
    /// indicates a loading error.
    /// </summary>
    public string Path { get; init; } = "";

    /// <summary>
    /// Offset into the <c>.dll/.ico/.exe</c> used to select an icon. For
    /// <c>.ico</c> files, this is always 0. We only use positive numbers as
    /// their count can be queried, but negative numbers are persistent across
    /// application upgrade versions.
    /// </summary>
    public int Index { get; init; } = 0;

    /// <returns>Serialized form of IconSource. This is used to store data in <c>desktop.ini</c>!</returns>
    public override string ToString()
    {
        return $"{Path},{Index}";
    }
}

/// <summary>
/// Contains the bitmap of a loaded icon and where it was loaded from.
/// </summary>
public record LoadedIcon
{
    /// <summary>
    /// Which file and offset into the file this icon comes from.
    /// </summary>
    public required IconSource Source { get; init; }

    /// <summary>
    /// Loaded image data.
    /// </summary>
    public required Bitmap Image { get; init; }

    /// <returns>Image size and where it was loaded from.</returns>
    public override string ToString()
    {
        return $"{Image.Size}@{Source}";
    }
}
