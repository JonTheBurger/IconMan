using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IconMan.Services;
using IconMan.ViewModels;
using IconMan.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace IconMan;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private IServiceProvider? Control;

    public static T GetService<T>() => ((App)Current).Control.GetRequiredService<T>();

    public override void OnFrameworkInitializationCompleted()
    {
        // Dependency Injection
        Control = new ServiceCollection()
            .AddSingleton(LoggerFactory.Create(builder => builder.AddDebug()))
            .AddSingleton<IIconService, Win32IconService>()
            .AddSingleton<IIniService, Win32IniService>()
            .BuildServiceProvider();

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
