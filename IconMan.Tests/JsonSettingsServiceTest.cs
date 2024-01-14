using IconMan.Services;

namespace IconMan.Tests;

public class JsonSettingsServiceTest
{
    [Fact]
    public void TestSerialization()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "IconMan.Tests");
        var service = new JsonSettingsService(tempDir);
        service.Clear();

        var recentDirs = service.Settings.RecentDirectories;
        Assert.Empty(recentDirs);

        recentDirs.Add("Dir1");
        recentDirs.Add("Dir2");
        recentDirs.Add("Dir3");
        service.Settings.RecentDirectories = recentDirs;

        Assert.Equal(service.Settings.RecentDirectories, recentDirs);
        service.Save();
    }
}
