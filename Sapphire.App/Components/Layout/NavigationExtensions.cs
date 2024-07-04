using Microsoft.AspNetCore.Components;

namespace Sapphire.App.Components.Layout;

public static class NavigationExtensions
{
    public static string GetSelectedStyle(this NavigationManager navigation, string pattern)
    {
        return IsSelected(navigation, pattern) ? "background-color: var(--mud-palette-lines-default) !important" : string.Empty;
    }

    public static bool IsSelected(this NavigationManager navigation, string pattern)
    {
        return navigation.ToBaseRelativePath(navigation.Uri).StartsWith(pattern[1..]);
    }
}