using Avalonia.Media.Imaging;
using IconMan.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IconMan.Services;

public class Win32IconService : IIconService
{
    public Win32IconService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<Win32IconService>();
    }

    public int GetBitmapCount(string file)
    {
        if (!File.Exists(file)) { throw new ArgumentOutOfRangeException(nameof(file)); }
        return ExtractIconExW(file, -1, 0, 0, 0);
    }

    public Bitmap GetBitmap(IconSource source)
    {
        if (!File.Exists(source.Path)) { throw new ArgumentException("File does not exist", nameof(source)); }
        if (source.Index < 0) { throw new ArgumentOutOfRangeException(nameof(source), source.Index, "Positive integer required"); }

        var result = ExtractIconExW(source.Path, source.Index, out IntPtr large, out IntPtr small, 1);
        if (result <= 0)
        {
            var win32 = new Win32Exception(Marshal.GetLastWin32Error());
            _logger.LogError("Failed ExtractIconExW with '{result}' and code '{nativeErrorr}': '{errorMessage}' for '{file}'", result, win32.NativeErrorCode, win32.Message, source.Path);
            throw win32;
        }

        System.Drawing.Icon unmanaged;
        if (large != 0)
        {
            if (small != 0)
            {
                DestroyIcon(small);
            }
            unmanaged = System.Drawing.Icon.FromHandle(large);
        }
        else
        {
            unmanaged = System.Drawing.Icon.FromHandle(small);
        }

        return ToAvaloniaBitmap(unmanaged);
    }

    private static Bitmap ToAvaloniaBitmap(System.Drawing.Icon icon)
    {
        var bitmap = icon.ToBitmap();
        var region = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(region, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var converted = new Bitmap(
            format: Avalonia.Platform.PixelFormat.Bgra8888,
            alphaFormat: Avalonia.Platform.AlphaFormat.Unpremul,
            data: data.Scan0,
            size: new Avalonia.PixelSize(data.Width, data.Height),
            dpi: new Avalonia.Vector(96, 96),
            stride: data.Stride
        );
        bitmap.UnlockBits(data);
        bitmap.Dispose();
        icon.Dispose();
        DestroyIcon(icon.Handle);
        return converted;
    }

    public async IAsyncEnumerable<LoadedIcon> GetIconsAsync(string file, [EnumeratorCancellation] CancellationToken token = default)
    {
        int count = await Task.Run(() => GetBitmapCount(file));
        for (int i = 0; ((i < count) && (!token.IsCancellationRequested)); i++)
        {
            IconSource source = new(Path: file, Index: i);
            yield return await Task.Run(() =>
            {
                return new LoadedIcon(Source: source, Image: GetBitmap(source));
            });
        }
    }

    // https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexw
    [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconExW(string lpszFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconsmall, int nIcons);

    // https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexw
    [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconExW(string lpszFile, int nIconIndex, IntPtr phiconLarge, IntPtr phiconsmall, int nIcons);

    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroyicon
    [DllImport("user32.dll", EntryPoint = "DestroyIcon", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private readonly ILogger _logger;
}
