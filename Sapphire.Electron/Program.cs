using ElectronNET.API;
using ElectronNET.API.Entities;
using Sapphire.Data;
using Sapphire.Electron.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHotKeys2();

builder.Services.AddSingleton(new DatabaseManager<ProjectDatabase>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


await app.StartAsync();

var options = new BrowserWindowOptions()
{
    MinHeight = 400,
    MinWidth = 800,
    Frame = false
};
var window = await Electron.WindowManager.CreateWindowAsync(options);
await window.WebContents.Session.ClearCacheAsync();

app.WaitForShutdown();