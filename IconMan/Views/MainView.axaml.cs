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

    private void Ligma()
    {

    }

    private async void AddIconSourceButton_Clicked(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add Icon Sources",
            AllowMultiple = true,
            FileTypeFilter = [
                new FilePickerFileType("Windows Icons")
                {
                    Patterns = new[] { "*.dll", "*.ico" }
                }
            ]
        });
        var vm = (MainViewModel)DataContext!;
        foreach (var file in files)
        {
            string? path = file.TryGetLocalPath();
            if (path != null)
            {
                vm.IconSources.Add(path);
            }
        }
    }
}
