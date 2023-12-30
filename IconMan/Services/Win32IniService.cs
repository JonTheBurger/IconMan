using System.Runtime.InteropServices;
using System.Text;

namespace IconMan.Services;

public class Win32IniService : IIniService
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern long GetProfileStringW(string Section, string Key, string Value, string FilePath);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
}
