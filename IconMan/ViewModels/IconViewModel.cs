using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;

namespace IconMan.ViewModels;

public partial class IconViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _image;

    [ObservableProperty]
    private string _path = "";

    [ObservableProperty]
    private int _index = 0;

    [ObservableProperty]
    private bool _isFavorite = false;
}

