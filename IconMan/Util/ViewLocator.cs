using Avalonia.Controls;
using Avalonia.Controls.Templates;
using IconMan.ViewModels;
using System;

namespace IconMan.Util;

#pragma warning disable IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
public class ViewLocator : IDataTemplate
{
    public bool SupportsRecycling => false;

    public Control Build(object? data)
    {
        Control view = _missing;

        if (data is not null)
        {
            Type? type = Type.GetType(data!.GetType()?.FullName?.Replace("ViewModel", "View") ?? "");

            if (type is not null)
            {
                view = Activator.CreateInstance(type) as Control ?? _missing;
            }
        }

        return view;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    private static readonly TextBlock _missing = new() { Text = "ERROR: Control Not Found" };
}
#pragma warning restore IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
