using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using WindowsBitmap = System.Drawing.Bitmap;

namespace IconMan.Services;

public class Win32IconService : IIconService
{
    public Win32IconService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<Win32IconService>();
    }

    public int GetIconCount(string file)
    {
        if (!File.Exists(file)) { throw new ArgumentOutOfRangeException(nameof(file)); }
        return ExtractIconExW(file, -1, 0, 0, 0);
    }

    public Icon GetIcon(string file, int index)
    {
        nint large;
        nint small;

        if (!File.Exists(file)) { throw new ArgumentOutOfRangeException(nameof(file)); }
        var result = ExtractIconExW(file, index, out large, out small, 1);
        if ((index < 0) || (result <= 0))
        {
            _logger.LogError($"Failed ExtractIconExW with code '{result}' for '{file}'");
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        _logger.LogInformation($"ExtractIconExW '{file}' at '{index}'");

        return Icon.FromHandle(large != 0 ? large : small);
    }

    public async IAsyncEnumerable<Icon> GetIconsAsync(string file, [EnumeratorCancellation] CancellationToken token = default)
    {
        await _semaphore.WaitAsync();
        try
        {
            int count = await Task.Run(() => GetIconCount(file));
            for (int i = 0; ((i < count) && (!token.IsCancellationRequested)); i++)
            {
                yield return await Task.Run(() => GetIcon(file, i));
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexw
    [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconExW(string lpszFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconsmall, int nIcons);

    // https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexw
    [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconExW(string lpszFile, int nIconIndex, IntPtr phiconLarge, IntPtr phiconsmall, int nIcons);

    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1);
}

public static class Win32IconExtensions
{
    public static AvaloniaBitmap ToAvaloniaBitmap(this Icon self)
    {
        return self.ToBitmap().ToAvalonia();
    }

    public static AvaloniaBitmap ToAvalonia(this WindowsBitmap b)
    {
        var self = new WindowsBitmap(b);
        var data = self.LockBits(new Rectangle(0, 0, self.Width, self.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var converted = new AvaloniaBitmap(
            format: Avalonia.Platform.PixelFormat.Bgra8888,
            alphaFormat: Avalonia.Platform.AlphaFormat.Unpremul,
            data: data.Scan0,
            size: new Avalonia.PixelSize(data.Width, data.Height),
            dpi: new Avalonia.Vector(96, 96),
            stride: data.Stride
        );
        self.UnlockBits(data);
        self.Dispose();
        return converted;
    }
}

