using Avalonia.Media.Imaging;
using IconMan.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IconMan.Services;

/// <summary>
/// Manages the details of loading icons from a <c>.ico/.dll</c> etc.
/// </summary>
public interface IIconService
{
    /// <summary>
    /// Determines how many icons are available from a given file. <c>.ico</c>
    /// files will always return 1.
    /// </summary>
    /// <param name="file">Path to existing <c>.ico/.dll</c>.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If a bad file was provided.</exception>
    /// <returns>The number of icons available for loading.</returns>
    int GetBitmapCount(string file);

    /// <summary>
    /// Loads the image data from a file at the provided offset. If the icon
    /// source provides both smaller and larger versions of the same icon, the
    /// larger version will be preferentially loaded.
    /// </summary>
    /// <param name="file">Path to existing <c>.ico/.dll</c> with a valid offset.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If a bad file was provided.</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">If a Windows error was encountered.</exception>
    /// <returns>Loaded image data.</returns>
    Bitmap GetBitmap(IconSource source);

    /// <summary>
    /// Non-throwing version of <see cref="GetBitmap(IconSource)"/>
    /// </summary>
    Bitmap? TryGetBitmap(IconSource source)
    {
        try
        {
            return GetBitmap(source);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Loads all available icons from a given <c>.ico/.dll</c> file without
    /// blocking the caller.
    /// </summary>
    /// <param name="file"><c>.ico/.dll</c> file. The number of available icons will be queried.</param>
    /// <param name="token">Allows the caller to cancel the operation early.</param>
    /// <returns>All icons in <paramref name="file"/></returns>
    IAsyncEnumerable<LoadedIcon> GetIconsAsync(string file, CancellationToken token = default);

    /// <summary>
    /// Loads all available icons from multiple provided <c>.ico/.dll</c> files
    /// without blocking the caller.
    /// </summary>
    /// <param name="files">List of files to load rom. The number of available icons will be queried.</param>
    /// <param name="token">Allows the caller to cancel the operation early.</param>
    /// <returns>All icons in <paramref name="file"/></returns>
    async IAsyncEnumerable<LoadedIcon> GetIconsAsync(IEnumerable<string> files, [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var file in files)
        {
            await foreach(var icon in GetIconsAsync(file, token))
            {
                yield return icon;
            }
        }
    }
}
