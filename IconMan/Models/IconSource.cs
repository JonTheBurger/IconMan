using System.Drawing;

namespace IconMan.Models;

public record IconSource(string Path = "", int Index = 0);
public record LoadedIcon(IconSource Source, Bitmap Image);
