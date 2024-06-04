using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.FluentUI.AspNetCore.Components;
using MudBlazor.Services;
using Sapphire.App.Services;
using Sapphire.Data;
using App = Sapphire.App.Components.App;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

var userData = await Electron.App.GetPathAsync(PathName.UserData);
var recentDatabaseStoragePath = Path.Join(userData, "recent_database_storage_path.json");
builder.Services.AddSingleton(new DatabaseService<ProjectDatabase>(recentDatabaseStoragePath));

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

await app.StartAsync();

var options = new BrowserWindowOptions()
{
    MinHeight = 800,
    MinWidth = 1400,
    Frame = false
};
var window = await Electron.WindowManager.CreateWindowAsync(options);
await window.WebContents.Session.ClearCacheAsync();

app.WaitForShutdown();