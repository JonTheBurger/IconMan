using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IconMan.Services;

public interface IIconService
{
    int GetIconCount(string file);
    Icon GetIcon(string file, int index);
    IAsyncEnumerable<Icon> GetIconsAsync(string file, [EnumeratorCancellation] CancellationToken token = default);
    async IAsyncEnumerable<Icon> GetIconsAsync(IEnumerable<string> files, [EnumeratorCancellation] CancellationToken token = default)
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
