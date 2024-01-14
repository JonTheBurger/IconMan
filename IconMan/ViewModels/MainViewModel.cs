using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconMan.Services;
using IconMan.Util;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IconMan.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel() : this(App.GetService<IIconService>(), App.GetService<ISettingsService>())
    {
    }

    public MainViewModel(IIconService iconService, ISettingsService settingsService)
    {
        _iconService = iconService;
        _settingsService = settingsService;

        RecentDirectories = new(_settingsService.Settings.RecentDirectories);
        IconSources.CollectionChanged += IconSources_CollectionChanged;  // TODO: Can this be made async?
        foreach (var src in _settingsService.Settings.IconSources)
        {
            IconSources.Add(src);
        }
    }

    public ObservableCollection<string> RecentDirectories { get; init; }
    public ObservableCollection<string> IconSources { get; } = [];
    public ObservableCollection<IconViewModel> Icons { get; } = [];

    [ObservableProperty]
    private string _selectedIconSource;

    //public void IconSource_Removed(object sender, RoutedEventArgs e)
    //[RelayCommand(CanExeute="CanExecute")]
    public void IconSource_Removed()
    {
    }

    private async void IconSources_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                // Our UI only allows adding to the end
                foreach (var item in e.NewItems)
                {
                    var path = (string)item!;
                    await foreach (var bitmap in _iconService.GetBitmapsAsync(path))
                    {
                        IconViewModel vm = new()
                        {
                            Path = path,
                            Image = bitmap
                        };
                        // TODO: Dialog on exception?
                        Icons.Add(vm);
                    }
                }
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                {
                    var path = (string)item!;
                    Icons.RemoveFirstRangeWhere(i => i.Path == path);
                }
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                throw new NotImplementedException();
            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                // We don't bother to re-order already loaded icons - any reorder will apply upon next reload
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                Icons.Clear();
                break;
        }
    }

    private readonly IIconService _iconService;
    private readonly ISettingsService _settingsService;
}
