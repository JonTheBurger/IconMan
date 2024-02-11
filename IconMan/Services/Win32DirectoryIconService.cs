using IconMan.Models;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IconMan.Services;

public class Win32DirectoryIconService : IDirectoryIconService
{
    public bool HasCustomIcon(string directory)
    {
        return GetCustomIcon(directory).Path != "";
    }

    public IconSource GetCustomIcon(string directory)
    {
        var iniPath = Path.Combine(directory, INI_FILE);
        if (!File.Exists(iniPath)) { return new(); }

        var lpReturnedString = new byte[PATH_LIMIT * 2]; // more than enough extra size for comma + int

        // TODO: sloppy
        int result = GetPrivateProfileStringW(SECTION, KEY, "", lpReturnedString, lpReturnedString.Length, iniPath);
        if (result == 0) { return new(); }

        string text = Encoding.Unicode.GetString(lpReturnedString);
        if (text == "") { return new(); }

        return IconSourceFromValue(text);
    }

    public void SetCustomIcon(string directory, IconSource icon)
    {
        var iniPath = Path.Combine(directory, INI_FILE);
        if (!Directory.Exists(directory)) { throw new IOException($"{directory} is not a valid directory"); }
        if (!File.Exists(iniPath)) { File.Create(iniPath); }

        string value = IconSourceToValue(icon);
        bool ok = WritePrivateProfileStringW(SECTION, KEY, value, iniPath);
    }

    public void ResetCustomIcon(string directory)
    {
        var iniPath = Path.Combine(directory, INI_FILE);
        if (!Directory.Exists(directory)) { throw new IOException($"{directory} is not a valid directory"); }
        if (!File.Exists(iniPath)) { File.Create(iniPath); }

        bool ok = WritePrivateProfileStringW(SECTION, KEY, null, iniPath);
    }

    public static readonly string INI_FILE = "desktop.ini";
    public static readonly string SECTION = ".ShellClassInfo";
    public static readonly string KEY = "IconResource";
    public static readonly int PATH_LIMIT = 260;

    public static IconSource IconSourceFromValue(string text)
    {
        var split = text.LastIndexOf(',');
        var path = text.Substring(0, split);
        var index = int.Parse(text.Substring(split + 1));
        return new IconSource(path, index);
    }

    public static string IconSourceToValue(IconSource icon)
    {
        return $"{icon.Path},{icon.Index}";
    }

    public static void ResetIconCache()
    {

    }

    public static void ProtectDesktopIni(string directory, bool protect = true)
    {
        var iniPath = Path.Combine(directory, "desktop.ini");
        if (protect)
        {
            File.SetAttributes(iniPath, File.GetAttributes(iniPath) | FileAttributes.Hidden | FileAttributes.System);
            File.SetAttributes(directory, File.GetAttributes(directory) | FileAttributes.System);
        }
        else
        {
            File.SetAttributes(iniPath, File.GetAttributes(iniPath) & ~(FileAttributes.Hidden | FileAttributes.System));
        }
    }

    // https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw
    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int GetPrivateProfileStringW(string lpAppName, string lpKeyName, string lpDefault, byte[] lpReturnedString, int nSize, string lpFileName);

    // https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringw
    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool WritePrivateProfileStringW(string lpAppName, string? lpKeyName, string? lpString, string lpFileName);
}
