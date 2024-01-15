using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconMan.Services;
using IconMan.Util;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        RecentDirectories = _settingsService.Settings.RecentDirectories;
        IconSources = _settingsService.Settings.IconSources;
        IconSources.CollectionChanged += IconSources_CollectionChanged;  // TODO: Can this be made async?
        foreach (var source in IconSources)
        {
            LoadIconsAsync(source);
        }
    }

    public ObservableCollection<string> RecentDirectories { get; init; }
    public ObservableCollection<string> IconSources { get; init; }
    public ObservableCollection<IconViewModel> Icons { get; } = [];

    [ObservableProperty]
    private int _selectedIconSourceIndex;

    [RelayCommand]
    public void IconSource_Removed(int d)
    {
        IconSources.RemoveAt(SelectedIconSourceIndex);
    }

    private async void IconSources_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                // Our UI only allows adding to the end
                foreach (var item in e.NewItems)
                {
                    var path = (string)item!;
                    LoadIconsAsync((string)item!);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                {
                    var path = (string)item!;
                    Icons.RemoveFirstRangeWhere(i => i.Path == path);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Move:
                // We don't bother to re-order already loaded icons - any reorder will apply upon next reload
                break;
            case NotifyCollectionChangedAction.Reset:
                Icons.Clear();
                break;
        }
    }

    private async Task LoadIconsAsync(string path)
    {
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

    private readonly IIconService _iconService;
    private readonly ISettingsService _settingsService;
}
