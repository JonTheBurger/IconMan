using Avalonia.Media.Imaging;
using IconMan.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IconMan.Services;

public interface IIconService
{
    int GetBitmapCount(string file);

    Bitmap GetBitmap(IconSource source);

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

    IAsyncEnumerable<LoadedIcon> GetIconsAsync(string file, CancellationToken token = default);

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
