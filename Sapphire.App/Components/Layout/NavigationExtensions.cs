using Microsoft.AspNetCore.Components;

namespace Sapphire.App.Components.Layout;

public static class NavigationExtensions
{
    public static string GetSelectedStyle(this NavigationManager navigation, string pattern)
    {
        return navigation.ToBaseRelativePath(navigation.Uri).StartsWith(pattern[1..]) 
            ? "background-color: var(--mud-palette-lines-default) !important" : string.Empty;
    }
}