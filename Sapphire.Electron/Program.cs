using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


await app.StartAsync();

var options = new BrowserWindowOptions()
{
    MinHeight = 600
};

var mainWindow = await Electron.WindowManager.CreateWindowAsync(options);
app.WaitForShutdown();