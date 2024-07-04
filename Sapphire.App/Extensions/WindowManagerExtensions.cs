using ElectronNET.API;

namespace Sapphire.App.Extensions;

public static class WindowManagerExtensions
{
    public static BrowserWindow? GetCurrent(this WindowManager windowManager)
    {
        return windowManager.BrowserWindows.SingleOrDefault();
    }
}