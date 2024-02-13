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

public class Win32IconService(ILoggerFactory loggerFactory) : IIconService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Win32IconService>();

    /// <inheritdoc/>
    public int GetBitmapCount(string file)
    {
        if (!File.Exists(file)) { throw new ArgumentOutOfRangeException(nameof(file)); }
        return ExtractIconExW(file, -1, 0, 0, 0);
    }

    /// <inheritdoc/>
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

        // A dll may contain both a large and small version of the same icon - prefer the large version.
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

    /// <summary>
    /// Disposes of the provided icon and converts it into an Avalonia bitmap.
    /// <see cref="System.Drawing.Icon.FromHandle(nint)"/> does not correctly
    /// dispose of itself, so we dispose of it ourselves immediately after
    /// converting.
    /// </summary>
    /// <param name="icon">Icon to convert and destroy. <b>DO NOT USE AFTER PASSING TO THIS FUNCTION</b>.</param>
    /// <returns>Avalonia bitmap.</returns>
    private static Bitmap ToAvaloniaBitmap(System.Drawing.Icon icon)
    {
        // https://github.com/AvaloniaUI/Avalonia/discussions/5908
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

    /// <inheritdoc/>
    public async IAsyncEnumerable<LoadedIcon> GetIconsAsync(string file, [EnumeratorCancellation] CancellationToken token = default)
    {
        int count = await Task.Run(() => GetBitmapCount(file));
        for (int i = 0; ((i < count) && (!token.IsCancellationRequested)); i++)
        {
            IconSource source = new() { Path = file, Index = i };
            yield return await Task.Run(() =>
            {
                return new LoadedIcon { Source = source, Image = GetBitmap(source) };
            });
        }
    }

    // Disable because we don't want to enable unsafe code.
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexw"/>
    /// </summary>
    [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconExW(string lpszFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconsmall, int nIcons);

    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexw"/>
    /// </summary>
    [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconExW(string lpszFile, int nIconIndex, IntPtr phiconLarge, IntPtr phiconsmall, int nIcons);

    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroyicon"/>
    /// </summary>
    [DllImport("user32.dll", EntryPoint = "DestroyIcon", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool DestroyIcon(IntPtr hIcon);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
}
