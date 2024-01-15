using IconMan.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;

namespace IconMan.Tests;

public class Win32IconServiceTest
{
    private const string DDORES_DLL = @"C:\Windows\System32\ddores.dll";
    private readonly ILoggerFactory _logger = new NullLoggerFactory();
    private readonly Win32IconService _service;
    public Win32IconServiceTest()
    {
        _service = new(_logger);
    }

    [Fact]
    public void GetIconCount_GreaterThanZero()
    {
        for (int i = 0; i < 100; ++i)
        {
            Assert.True(_service.GetIconCount(DDORES_DLL) > 1);
        }
    }

    [Fact]
    public void GetIconsSynchronously_StressTest()
    {
        for (int stress = 0; stress < 100; ++stress)
        {
            for (int i = 0; (i < _service.GetIconCount(DDORES_DLL)); ++i)
            {
                var icon = _service.GetIcon(DDORES_DLL, i);
                Assert.True(icon.Handle > 0);
            }
        }
    }

    [Fact]
    public async Task GetIconsAsync_()
    {
        for (int i = 0; i < 100; ++i)
        {
            await foreach (var icon in _service.GetIconsAsync(DDORES_DLL))
            {
                Assert.True(icon.Handle > 0);
            }
        }
    }

    [Fact]
    public async Task GetBitmapsAsync_StressTest()
    {
        for (int stress = 0; stress < 100; ++stress)
        {
            await foreach(var bitmap in _service.GetBitmapsAsync(DDORES_DLL))
            {
                Assert.True(bitmap.Size.Width > 0);
                Assert.True(bitmap.Size.Height > 0);
            }
        }
    }
}
