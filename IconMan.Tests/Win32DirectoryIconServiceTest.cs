using IconMan.Services;

namespace IconMan.Tests;

public class Win32DirectoryIconServiceTest
{
    [Fact]
    public void ExampleTest()
    {
        var service = new Win32DirectoryIconService();
        service.GetCustomIcon(@"C:\1");
        //Assert.True(service.HasCustomIcon(@"C:\1"));
    }
}