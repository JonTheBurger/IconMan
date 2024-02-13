// File for testing out Win32 API function calls; requires >= C++17
#include <iostream>
#include <iomanip>
#include <windows.h>

#define WSTRINGIFY(expr) L#expr
#define TAB(expr, width) Tab(WSTRINGIFY(expr), expr, width)
static_assert(sizeof(HICON) == sizeof(int*), "HICON != int*??");

template <typename T>
class Tab {
    const wchar_t* _name;
    const T&       _contents;
    int            _width;
public:
    Tab(const wchar_t* name, const T& contents, int width = 0)
        : _name{name}
        , _contents{contents}
        , _width{width}
    {
    }

    friend std::wostream& operator<<(std::wostream& os, const Tab& section)
    {
        os << ' ';
        os << section._name;
        os << ": ";
        if (section._width)
        {
            os << std::setw(section._width);
        }
        os << section._contents;
        os << " |";
        return os;
    }
};

int main()
{
    static const wchar_t* const DLL = L"C:\\Windows\\System32\\shell32.dll";
    static const int STRESS = 32;

    HICON loicon;
    HICON hiicon;

    std::wcout << L"| ExtractIconExW |" << TAB(DLL, 4) << TAB(STRESS, 4) << std::endl;

    for (int stress = 0; stress < STRESS; ++stress)
    {
        for (int idx = 0; idx < ExtractIconExW(DLL, -1, nullptr, nullptr, 0); ++idx)
        {
            if (idx < 0)
            {
                DWORD error = GetLastError();
                std::wcout << L'|' << Tab(L"ERROR", L"ExtractIcon(-1)", 15) << TAB(error, 8) << TAB(idx, 4) << TAB(stress, 4) << L"\r\n";
                continue;
            }

            if (int result = ExtractIconExW(DLL, idx, &loicon, &hiicon, 1); result <= 0)
            {
                DWORD error = GetLastError();
                std::wcout << L'|' << Tab(L"ERROR", L"ExtractIconExW", 15) << TAB(error, 8) << TAB(idx, 4) << TAB(stress, 4) << TAB(result, 4) << L"\r\n";
                continue;
            }

            if (!DestroyIcon(hiicon))
            {
                DWORD error = GetLastError();
                std::wcout << L'|' << Tab(L"ERROR", L"DestroyIcon(hi)", 15) << TAB(error, 8) << TAB(idx, 4) << TAB(stress, 4) << L"\r\n";
            }

            if (!DestroyIcon(loicon))
            {
                DWORD error = GetLastError();
                std::wcout << L'|' << Tab(L"ERROR", L"DestroyIcon(lo)", 15) << TAB(error, 8) << TAB(idx, 4) << TAB(stress, 4) << L"\r\n";
            }
        }
    }
}
