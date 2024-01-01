using IconMan.Models;

namespace IconMan.Services;

public interface IDirectoryIconService
{
    bool HasCustomIcon(string directory);
    IconSource GetCustomIcon(string directory);
    void SetCustomIcon(string directory, IconSource icon);
    void ResetCustomIcon(string directory);
}
