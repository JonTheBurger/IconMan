using CommunityToolkit.Mvvm.Input;
using IconMan.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IconMan.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel() : this(App.GetService<IIconService>())
    {
    }

    public MainViewModel(IIconService iconService)
    {
        _iconService = iconService;
        LoadButtonCommand = new AsyncRelayCommand(LoadButtonClicked);
    }

    public string ButtonText => "Load Icons";

    public ObservableCollection<IconViewModel> Icons { get; } = new();

#pragma warning disable S1075 // URIs should not be hardcoded
    public readonly string[] DefaultIconFiles = [
        @"C:\Windows\System32\imageres.dll",
        @"C:\Windows\System32\shell32.dll",
        @"C:\Windows\System32\ddores.dll",
        @"C:\Windows\System32\mmres.dll",
        @"C:\Windows\System32\netshell.dll",
        @"C:\Windows\System32\networkexplorer.dll",
        @"C:\Windows\System32\setupapi.dll",
        @"C:\Windows\System32\wmploc.dll",
        @"C:\Windows\System32\wpdshext.dll",
        @"C:\Windows\System32\dsuiext.dll",
        @"C:\Windows\System32\comres.dll",
    ];
#pragma warning restore S1075 // URIs should not be hardcoded

    public IAsyncRelayCommand LoadButtonCommand { get; init; }

    public async Task LoadButtonClicked()
    {
        await foreach (var icon in _iconService.GetIconsAsync(DefaultIconFiles))
        {
            try
            {
                IconViewModel vm = new();
                vm.Image = icon.ToAvaloniaBitmap();
                Icons.Add(vm);
            }
            catch (ArgumentException ex) {

            }
        }
    }

    private readonly IIconService _iconService;
}
