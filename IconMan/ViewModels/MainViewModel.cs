using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconMan.Models;
using IconMan.Services;
using IconMan.Util;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IconMan.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel() : this(App.GetService<IIconService>(), App.GetService<ISettingsService>(), App.GetService<IDirectoryIconService>())
    {
    }

    public MainViewModel(IIconService iconService, ISettingsService settingsService, IDirectoryIconService directoryIconService)
    {
        _iconService = iconService;
        _settingsService = settingsService;
        _directoryIconService = directoryIconService;

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
    private int _selectedIconIndex;

    [ObservableProperty]
    private int _selectedIconSourceIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentDirImage))]
    private string? _currentDirPath = null;

    public Bitmap? CurrentDirImage
    {
        get
        {
            if ((CurrentDirPath != null) && (_directoryIconService.HasCustomIcon(CurrentDirPath)))
            {
                var iconSource = _directoryIconService.GetCustomIcon(CurrentDirPath);
                return _iconService.TryGetBitmap(iconSource);
            }
            return null;
        }
    }

    //[RelayCommand(CanExecute = Selection)]
    [RelayCommand]
    public void OverwriteDirectoryIcon()
    {
        var icon = Icons[SelectedIconIndex];
        var source = new IconSource { Path = icon.Path, Index = icon.Index };
        _directoryIconService.SetCustomIcon(CurrentDirPath, source);
        // TODO: sloppy
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentDirImage)));
    }


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
        // TODO FIXME WARNING ERROR: The index isn't actually set lol
        await foreach (var loadedIcon in _iconService.GetIconsAsync(path))
        {
            IconViewModel vm = new()
            {
                Path = loadedIcon.Source.Path,
                Index = loadedIcon.Source.Index,
                Image = loadedIcon.Image,
            };
            // TODO: Dialog on exception?
            Icons.Add(vm);
        }
    }

    private async Task LoadCurrentDirIcon(string directory)
    {

    }

    private readonly IIconService _iconService;
    private readonly ISettingsService _settingsService;
    private readonly IDirectoryIconService _directoryIconService;
}
