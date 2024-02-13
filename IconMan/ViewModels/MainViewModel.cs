using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconMan.Services;
using IconMan.Util;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IconMan.ViewModels;

/// <summary>
/// Display glue for the top-level display control.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IIconService _iconService;
    private readonly ISettingsService _settingsService;
    private readonly IDirectoryIconService _directoryIconService;

    /// <summary>
    /// Load services from IoC container.
    /// </summary>
    public MainViewModel() : this(App.GetService<IIconService>(), App.GetService<ISettingsService>(), App.GetService<IDirectoryIconService>())
    {
    }

    /// <summary>
    /// Manually specify services for testing.
    /// </summary>
    /// <param name="iconService">For example, <see cref="Win32IconService"/></param>
    /// <param name="settingsService">For example, <see cref="JsonSettingsService"/></param>
    /// <param name="directoryIconService">For example, <see cref="Win32DirectoryIconService"/></param>
    public MainViewModel(IIconService iconService, ISettingsService settingsService, IDirectoryIconService directoryIconService)
    {
        _iconService = iconService;
        _settingsService = settingsService;
        _directoryIconService = directoryIconService;

        IconSources = _settingsService.Settings.IconSources;
        IconSources.CollectionChanged += OnIconSourcesChanged;
        foreach (var source in IconSources)
        {
            LoadIconsAsync(source);
        }
    }

    /// <summary>
    /// List of <c>.dll/.ico</c> files used for customizing directories.
    /// Persists across sessions. Adding/Removing an IconSource will
    /// automatically Load/Unload its icons.
    /// </summary>
    public ObservableCollection<string> IconSources { get; init; }

    /// <summary>
    /// Selected <see cref="IconSources"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveIconSourceCommand))]
    private int _selectedIconSourceIndex = -1;

    /// <summary>
    /// Icons loaded from <see cref="IconSources"./>
    /// </summary>
    public ObservableCollection<IconViewModel> Icons { get; } = [];

    /// <summary>
    /// Selected <see cref="Icons"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OverwriteDirectoryIconCommand))]
    private int _selectedIconIndex = -1;

    /// <summary>
    /// The directory whose icon is being customized.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OverwriteDirectoryIconCommand))]
    [NotifyPropertyChangedFor(nameof(CurrentDirImage))]
    private string? _currentDirPath = null;

    /// <summary>
    /// Custom icon currently set for the given directory. This is null if
    /// there is no custom icon.
    /// </summary>
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

    /// <summary>
    /// Set the custom icon on a directory using the currently selected IconSource.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOverwriteDirectoryIcon))]
    public void OverwriteDirectoryIcon()
    {
        var icon = Icons[SelectedIconIndex].Icon;
        _directoryIconService.SetCustomIcon(CurrentDirPath!, icon.Source);
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentDirImage)));
    }

    /// <summary>
    /// Checks to see if Command for <see cref="OverwriteDirectoryIcon"/> can execute.
    /// </summary>
    private bool CanOverwriteDirectoryIcon()
    {
        return (CurrentDirPath != null) && (SelectedIconIndex > -1);
    }

    /// <summary>
    /// Removes an Icon source <c>.dll/.ico</c> and its loaded icons.
    /// </summary>
    /// <param name="index">index of selected IconSource</param>
    [RelayCommand(CanExecute = nameof(CanRemoveIconSource))]
    public void RemoveIconSource(int index)
    {
        IconSources.RemoveAt(SelectedIconSourceIndex);
    }

    /// <summary>
    /// Checks to see if Command for <see cref="RemoveIconSource"/> can execute.
    /// </summary>
    private bool CanRemoveIconSource()
    {
        return SelectedIconSourceIndex > -1;
    }

    /// <summary>
    /// Adds a new <c>.dll/.ico</c> file where icons shall be loaded from.
    /// </summary>
    /// <param name="path">File to add to icon load sources.</param>
    /// <returns>true if the source could be added</returns>
    public bool AddIconSource(string path)
    {
        if (IconSources.Contains(path))
        {
            return false;
        }
        IconSources.Add(path);
        return true;
    }

    /// <summary>
    /// Loads/Unloads icons when an Icon source <c>.dll/.ico</c> has been added or removed.
    /// </summary>
    /// <param name="sender">Event originator; unused.</param>
    /// <param name="e">How the collection has been changed.</param>
    /// <exception cref="NotImplementedException">We don't support Replace.</exception>
    private async void OnIconSourcesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                // Our UI only allows adding to the end
                foreach (var item in (e.NewItems ?? Array.Empty<string>()))
                {
                    var path = (string)item!;
                    await LoadIconsAsync((string)item!);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in (e.OldItems ?? Array.Empty<string>()))
                {
                    var path = (string)item!;
                    Icons.RemoveFirstRangeWhere(i => i.Icon.Source.Path == path);
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

    /// <summary>
    /// Loads icons from a path and adds the resulting ViewModel to <see cref="Icons"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Task indicating completion.</returns>
    private async Task LoadIconsAsync(string path)
    {
        await foreach (var icon in _iconService.GetIconsAsync(path))
        {
            Icons.Add(new(icon));
        }
    }
}
