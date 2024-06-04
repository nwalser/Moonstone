using MudBlazor;
using MudBlazor.Utilities;

namespace Sapphire.App.Components.Layout;

public class Theme
{
    private static readonly MudColor LinesDark = new(39, 39, 42, 255);
    private static readonly MudColor LinesLight = new(39, 39, 42, 60);
    private static readonly MudColor White = new(250, 250, 250, 255);
    private static readonly MudColor Black = new(9, 9, 11, 255);

    public static MudTheme Default => new()
    {
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "3px",
        },
        Palette = new PaletteLight()
        {
            Background = White,
            Surface = White,
            AppbarBackground = White,
            TextPrimary = Black,
            Primary = Black,
            AppbarText = Black,
            ActionDefault = Black,
            LinesDefault = LinesLight,
            LinesInputs = LinesLight,
            TableLines = LinesLight
        },
        PaletteDark = new PaletteDark
        {
            Background = Black,
            Surface = Black,
            AppbarBackground = Black,
            TextPrimary = White,
            Primary = White,
            AppbarText = White,
            ActionDefault = White,
            LinesDefault = LinesDark,
            LinesInputs = LinesDark,
            TableLines = LinesDark
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "Roboto" },
                FontWeight = 100,
                FontSize = "0.875rem"
            },
            H1 = new H1
            {
                FontSize = "1.25rem"
            },
            H2 = new H2
            {
                FontSize = "1rem"
            },
            H3 = new H3
            {
                FontSize = "1.1rem",
                FontWeight = 100
            },
            H4 = new H4
            {
                FontSize = "1rem",
                FontWeight = 100
            },
            Button = new Button
            {
                FontWeight = 400
            }
        }
    };
}