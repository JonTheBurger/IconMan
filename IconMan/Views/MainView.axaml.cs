using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using IconMan.ViewModels;

namespace IconMan.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void OpenDirectoryButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        var selection = await top!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open directory to customize its icon",
            AllowMultiple = false,
        });
        var vm = (MainViewModel)DataContext!;
        if (selection.Count > 0)
        {
            var dir = selection[0];
            string? path = dir.TryGetLocalPath();
            if (path != null)
            {
                vm.CurrentDirPath = path;
            }
        }
    }

    private async void AddIconSourceButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        var files = await top!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add Icon Sources",
            AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("Windows Icons") { Patterns = WindowsIconFiles }]
        });
        var vm = (MainViewModel)DataContext!;
        foreach (var file in files)
        {
            string? path = file.TryGetLocalPath();
            if (path != null)
            {
                vm.AddIconSource(path);
            }
        }
    }

    private static readonly string[] WindowsIconFiles = ["*.dll", "*.ico", "*.exe"];
}
