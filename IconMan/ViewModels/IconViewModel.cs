using CommunityToolkit.Mvvm.ComponentModel;
using IconMan.Models;

namespace IconMan.ViewModels;

/// <summary>
/// Display glue for an Icon loaded from a particular location. Many of these
/// will be used within a ListView.
/// </summary>
/// <param name="icon">Icon loaded from disk.</param>
public partial class IconViewModel(LoadedIcon icon) : ViewModelBase
{
    /// <summary>
    /// Corresponds to the icon and tooltip on the GUI.
    /// </summary>
    [ObservableProperty]
    private LoadedIcon _icon = icon;

    /// <summary>
    /// Corresponds to a favorite star on the GUI.
    /// </summary>
    [ObservableProperty]
    private bool _isFavorite = false;
}

