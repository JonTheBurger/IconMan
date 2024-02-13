using IconMan.Models;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IconMan.Services;

public class Win32DirectoryIconService : IDirectoryIconService
{
    /// <inheritdoc/>
    public bool HasCustomIcon(string directory)
    {
        return GetCustomIcon(directory).Path != "";
    }

    /// <inheritdoc/>
    public IconSource GetCustomIcon(string directory)
    {
        var iniPath = Path.Combine(directory, INI_FILE);
        if (!File.Exists(iniPath)) { return new(); }

        // More than enough extra size for additional comma + int index.
        var lpReturnedString = new byte[PATH_LIMIT * 2];

        // Load value for Section [.ShellClassInfo], Key IconResource, and return an empty string if it doesn't exist.
        int result = GetPrivateProfileStringW(SECTION, KEY, "", lpReturnedString, lpReturnedString.Length, iniPath);
        if (result == 0) { return new(); }

        string text = Encoding.Unicode.GetString(lpReturnedString);
        if (text == "") { return new(); }

        return IconSourceFromValue(text);
    }

    /// <inheritdoc/>
    public void SetCustomIcon(string directory, IconSource icon)
    {
        var iniPath = Path.Combine(directory, INI_FILE);
        if (!Directory.Exists(directory)) { throw new IOException($"{directory} is not a valid directory"); }
        if (!File.Exists(iniPath)) { File.Create(iniPath); }

        string value = icon.ToString();
        try
        {
            ProtectDesktopIni(iniPath, false);
            WritePrivateProfileStringW(SECTION, KEY, value, iniPath);
        }
        finally
        {
            ProtectDesktopIni(iniPath);
        }
    }

    /// <inheritdoc/>
    public void ResetCustomIcon(string directory)
    {
        var iniPath = Path.Combine(directory, INI_FILE);
        if (!Directory.Exists(directory)) { throw new IOException($"{directory} is not a valid directory"); }
        if (!File.Exists(iniPath)) { File.Create(iniPath); }

        // null key clears the key/value.
        WritePrivateProfileStringW(SECTION, KEY, null, iniPath);
    }

    /// <summary>Windows directory icon settings are stored in <c>desktop.ini</c></summary>
    public static readonly string INI_FILE = "desktop.ini";
    /// <summary>Windows directory icon settings are stored in the <c>[.ShellClassInfo]</c> section.</c></summary>
    public static readonly string SECTION = ".ShellClassInfo";
    /// <summary>Windows directory icon settings are stored in the <c>IconResource=</c> key.</c></summary>
    public static readonly string KEY = "IconResource";
    /// <summary>
    /// Things can get weird on Windows if you go beyond the path limit. We
    /// technically do in order to allocate space for <c>,123</c> - the icon
    /// load index.
    /// </summary>
    public static readonly int PATH_LIMIT = 260;

    /// <summary>
    /// Parse file path and index from the string value from e.g. <c>IconResource=C:\Some\Path.dll,123</c>.
    /// This file expects that nobody screwed up a valid file - otherwise an exception will be thrown.
    /// </summary>
    /// <param name="text">Value text from <c>desktop.ini;[.ShellClassInfo];IconResource</c></param>
    /// <returns>Parsed path and index.</returns>
    public static IconSource IconSourceFromValue(string text)
    {
        var split = text.LastIndexOf(',');
        var path = text[..split];
        var index = int.Parse(text[(split + 1)..]);
        return new IconSource { Path = path, Index = index };
    }

    /// <summary>
    /// Un/Protects a system file such as <c>desktop.ini</c>.
    /// </summary>
    /// <param name="iniPath">File to un/protect; must exist.</param>
    /// <param name="protect">True to protect the <c>desktop.ini</c>, false to un-protect.</param>
    public static void ProtectDesktopIni(string iniPath, bool protect = true)
    {
        if (protect)
        {
            File.SetAttributes(iniPath, File.GetAttributes(iniPath) | FileAttributes.Hidden | FileAttributes.System);
        }
        else
        {
            File.SetAttributes(iniPath, File.GetAttributes(iniPath) & ~(FileAttributes.Hidden | FileAttributes.System));
        }
    }

    // Disable because we don't want to enable unsafe code.
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw"/>
    /// </summary>
    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int GetPrivateProfileStringW(string lpAppName, string lpKeyName, string lpDefault, byte[] lpReturnedString, int nSize, string lpFileName);

    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringw"/>
    /// </summary>
    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool WritePrivateProfileStringW(string lpAppName, string? lpKeyName, string? lpString, string lpFileName);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
}
