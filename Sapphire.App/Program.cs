using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.FluentUI.AspNetCore.Components;
using MudBlazor.Services;
using Sapphire.App.Services;
using Sapphire.Data;
using Sapphire.Data.AppDb;
using App = Sapphire.App.Components.App;

var builder = WebApplication.CreateBuilder(args);
var webRuntime = Convert.ToBoolean(Environment.GetEnvironmentVariable("WEB"));

builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

if (!webRuntime)
{
    var userData = await Electron.App.GetPathAsync(PathName.UserData);
    var appDbStoragePath = Path.Join(userData, "appdb.json");
    builder.Services.AddSingleton(new AppDb(appDbStoragePath));
    
    builder.Services.AddSingleton<DatabaseService<ProjectDatabase>>();
}
else
{
    builder.Services.AddSingleton<DatabaseService<ProjectDatabase>>();
}

builder.Services.AddSingleton(new TimeZoneService());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (!webRuntime)
{
    await app.StartAsync();

    var options = new BrowserWindowOptions()
    {
        Title = "Sapphire",
        MinHeight = 800,
        MinWidth = 1400,
        Frame = false
    };
    var window = await Electron.WindowManager.CreateWindowAsync(options);
    await window.WebContents.Session.ClearCacheAsync();

    app.WaitForShutdown();
}
else
{
    app.Run();
}
