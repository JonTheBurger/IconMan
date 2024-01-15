﻿using IconMan.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IconMan.Services;

public class Settings
{
    public ObservableCollection<string> RecentDirectories { get; set; } = [];
    public ObservableCollection<string> IconSources { get; set; } = [
        @"C:\Windows\System32\imageres.dll",
        @"C:\Windows\System32\shell32.dll",
        @"C:\Windows\System32\ddores.dll",
        @"C:\Windows\System32\mmres.dll",
        @"C:\Windows\System32\netshell.dll",
        @"C:\Windows\System32\networkexplorer.dll",
        @"C:\Windows\System32\setupapi.dll",
        @"C:\Windows\System32\wmploc.dll",
        @"C:\Windows\System32\wpdshext.dll",
        @"C:\Windows\System32\dsuiext.dll",
        @"C:\Windows\System32\comres.dll",
    ];
    public ObservableCollection<IconSource> FavoriteIcons { get; set; } = [];
}

public interface ISettingsService
{
    Settings Settings { get; }
    void Save();
    void Load();
    void Clear();
}
