using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;

namespace IconMan.ViewModels;

public partial class IconViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap
        ? _image;

    [ObservableProperty]
    private string _path = "";

    [ObservableProperty]
    private int _index = 0;

    [ObservableProperty]
    private bool _isFavorite = false;
}

public record IconResource(string Path, int Index);

public class IniFile
{
    public static IconResource? GetIconResource(string directory)
    {
        IconResource? resource = null;

        string text = File.ReadAllLines(Path.Combine(directory, "desktop.ini"))
            .SkipWhile(line => !line.StartsWith("[.ShellClassInfo]"))
            .Skip(1)
            .TakeWhile(line => !line.StartsWith('['))
            .Where(line => line.StartsWith("IconResource="))
            .FirstOrDefault("IconResource=")
            .Substring("IconResource=".Length);

        if (text != "")
        {
            var split = text.LastIndexOf(',');
            var path = text.Substring(0, split);
            var index = int.Parse(text.Substring(split + 1));
            resource = new IconResource(path, index);
        }

        return resource;
    }
}
