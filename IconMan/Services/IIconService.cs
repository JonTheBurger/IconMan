using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IconMan.Services;

public interface IIconService
{
    int GetBitmapCount(string file);

    Bitmap GetBitmap(string file, int index);

    Bitmap? TryGetBitmap(string file, int index)
    {
        try
        {
            return GetBitmap(file, index);
        }
        catch
        {
            return null;
        }
    }

    IAsyncEnumerable<Bitmap> GetBitmapsAsync(string file, CancellationToken token = default);

    async IAsyncEnumerable<Bitmap> GetBitmapsAsync(IEnumerable<string> files, [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var file in files)
        {
            await foreach(var bitmap in GetBitmapsAsync(file, token))
            {
                yield return bitmap;
            }
        }
    }
}
