# IconMan

IconMan allows users to conveniently set custom icons for their Windowd
directories.

TODO: There will be a cool `.mp4` here at _some point_.

Usually, this process requires several tedious steps:

1. Right click a folder
2. Click "Show More Options" (on Windows 11)
3. Click "Properties"
4. Select the "Customize" tab
5. Click "Change Icon..."
6. Select a `.dll` or `.ico` file containing the desired icon
7. Click OK

Notably, only one `.dll` or `.ico` file can be viewed at a time, and only ~28
icons are visible on screen at once.

By default, IconMan loads several `.dll`s that contain useful icons, and allows
you to view as many as your screen resolution permits. Users can add additional
`.dll` or `.ico` files, and IconMan will remember them next time by storing the
icon sources in `%LocalAppData%\IconMan`. Frequently used icons can be
favorited for convenience.

IconMan (like many men) isn't pretty, but it gets the job done 😎.

# Installation

Simply download the exe from the github releases and run!

# CI

## Build

```ps1
dotnet build
```

## Bundle

## Test

## Lint

# Implementation Notes

IconMan uses the MVVM architecture via `Avalonia` and `Mvvm.CommunityToolkit`.
The Win32 API is used to load icons and modify `destktop.ini`, which stores
custom icon information. (System files such as these are hidden by default.)
The `desktop.ini` can store custom icon information in a few ways, but we
support the simplest, which is the form I have observed in the wild:

```
[.ShellClassInfo]
IconResource=C:\Windows\System32\imageres.dll,221
```

In some circumstances, the shell icon cache needs to be invalidated for
customized icons to register. My testing on Windows 11 suggests that
`explorer.exe` is pretty good about doing this automatically, so we don't
bother with it.

## Filesystem

The directory hierarchy is as follows:

```bash
|-- IconMan/         # IconMan library linked by Desktop exe & Tests
|   |-- Assets/      # Resources including icons & svg dimensions
|   |-- Models/      # Plain old data used by Services
|   |-- Services/    # Interface for reading system state into Models
|   |-- Util/        # Miscellaneous helper code
|   |-- ViewModels/  # Data binding between View <=> Serivce/Models
|   `-- Views/       # XAML files
|-- IconMan.Desktop/ # Desktop exe project; not very interesting
`-- IconMan.Tests/   # Automated sanity tests
```

## Services

Services are registered to the IoC container in `IconMan/App.axaml.cs`. Each
service has an interface to assist with testing. `IDirectoryIconService` is
manages the custom icons on directories, `IIconService` loads icons from `.dll`
and `.ico` files, and `ISettingsService` persistently stores used `.dll`s and
favorited icons.

## Models

On Windows, an icon is loaded by offsetting into a `.dll` by index, starting at
0. Files can be queried for the number of icons they contain, but icon indices
can change across application versions. Some files provide stable identifiers
for icons at negative numbers, but we don't support these.

## Views

`MainView.axaml.cs` contains the only code-behind for opening file dialogs.
`IconView.xaml` is used to show a single loaded icon and allow users to
register it as a favorite.

# Roadmap

**Fancy TODOs:**

- desktop.ini needs to be set as a system file
- Favorite icons
- Disallow duplicate IconSources
- dotnet format + xaml + editorconfig
- AOT standalone binary
- Github actions
- Windows Defender upload
- New Icon
- Test Avalonia Types https://docs.avaloniaui.net/docs/concepts/headless/headless-xunit
- Design-time data https://docs.avaloniaui.net/docs/guides/implementation-guides/how-to-use-design-time-data