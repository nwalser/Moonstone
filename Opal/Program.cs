using Microsoft.EntityFrameworkCore;
using Opal.Cache;
using Opal.Log;

var workspacePath = @"C:\Users\NathanielWalser\OneDrive - esp-engineering gmbh\Moonstone\workspace4";
var sessionId = Guid.Parse("794dcb19-a00e-4f5a-9eeb-5a2d3b582f60");
var cachePath = @"C:\Users\NathanielWalser\Desktop\temp";

var mutationsPath = Path.Join(workspacePath, "mutations");

var optionsBuilder = new DbContextOptionsBuilder<CacheContext>()
    .UseSqlite($"Data Source={cachePath}");
var store = new CacheContext(optionsBuilder.Options);


var streamSync = new StreamSync(mutationsPath, store);
streamSync.Initialize();



