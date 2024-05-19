using Amber.App.Services;
using Amber.Domain.Documents.Project;
using Amber.Domain.Documents.Todo;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Moonstone;
using MudBlazor.Services;

namespace Amber.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton(FolderPicker.Default);
            builder.Services.AddMudServices();
            
            builder.Services.AddSingleton(new Workspaces());
            builder.Services.AddSingleton(ProjectHandler.GetReader("session1"));
            builder.Services.AddSingleton(TodoHandler.GetReader("session1"));
            
#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
