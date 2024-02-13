using IconMan.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IconMan.Tests;

public class Win32IconServiceTest
{
#if WINDOWS
    private const string DDORES_DLL = @"C:\Windows\System32\ddores.dll";
    private readonly ILoggerFactory _logger = new NullLoggerFactory();
    private readonly Win32IconService _service;
    public Win32IconServiceTest()
    {
        _service = new(_logger);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetIconCount_GreaterThanZero()
    {
        for (int i = 0; i < 100; ++i)
        {
            Assert.True(_service.GetBitmapCount(DDORES_DLL) > 1);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBitmapsAsync_StressTest()
    {
        for (int stress = 0; stress < 100; ++stress)
        {
            await foreach(var bitmap in _service.GetIconsAsync(DDORES_DLL))
            {
                Assert.True(bitmap.Image.Size.Width > 0);
                Assert.True(bitmap.Image.Size.Height > 0);
            }
        }
    }
#endif
}
