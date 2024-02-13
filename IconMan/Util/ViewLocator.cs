using Avalonia.Controls;
using Avalonia.Controls.Templates;
using IconMan.ViewModels;
using System;

namespace IconMan.Util;

#pragma warning disable IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
/// <summary>
/// Looks up a View that corresponds with a given ViewModel.
/// <a href="https://docs.avaloniaui.net/docs/concepts/view-locator"/>
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Create a new View upon each call.
    /// </summary>
    public bool SupportsRecycling => false;

    /// <summary>
    /// Create a View that corresponds to the given ViewModel. "IconMan.ViewModels.IconViewModel" -> "IconMan.Views.IconView".
    /// </summary>
    /// <param name="data">View Model.</param>
    /// <returns>A corresponding view, or a text block containing an error message.</returns>
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

    /// <param name="data">Prospective View Model used for looking up a View.</param>
    /// <returns>True if <paramref name="data"/> is a ViewModel.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    /// <summary>
    /// Widget to show by default if we can't find a View for a given ViewModel.
    /// </summary>
    private static readonly TextBlock _missing = new() { Text = "ERROR: Control Not Found" };
}
#pragma warning restore IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
