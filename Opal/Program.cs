

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Opal.Cache;

var workspacePath = @"C:\Users\NathanielWalser\OneDrive - esp-engineering gmbh\Moonstone\workspace4";
var sessionId = Guid.Parse("794dcb19-a00e-4f5a-9eeb-5a2d3b582f60");
var cachePath = @"C:\Users\NathanielWalser\Desktop\temp";

var mutationsPath = Path.Join(workspacePath, "mutations");

var optionsBuilder = new DbContextOptionsBuilder<CacheContext>()
    .UseSqlite($"Data Source={cachePath}");
var store = new CacheContext(optionsBuilder.Options);

var changedFiles = new ConcurrentQueue<MutationFile>();

// create filesystem watcher
var fileSystemWatcher = new FileSystemWatcher
{
    Path = mutationsPath,
    IncludeSubdirectories = true,
    EnableRaisingEvents = true,
    NotifyFilter = NotifyFilters.LastWrite,
};
fileSystemWatcher.Created += FileChanged;
fileSystemWatcher.Changed += FileChanged;



return;

void FileChanged(object sender, FileSystemEventArgs e)
{
    if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
        return;

    var filename = Path.GetFileNameWithoutExtension(e.FullPath);
    var mutationFile = MutationFile.ParseFromFilename(filename);
    
    changedFiles.Enqueue(mutationFile);
}


void UpdateFile(MutationFile file)
{
    
}


public record MutationFile
{
    public Guid SessionId { get; init; }
    public Guid FileId { get; init; }

    
    public string GetFilename()
    {
        return $"{SessionId}-{FileId}";
    }
    
    public static MutationFile ParseFromFilename(string filename)
    {
        var splitted = filename.Split("-");
        
        return new MutationFile()
        {
            SessionId = Guid.Parse(splitted[0]),
            FileId = Guid.Parse(splitted[1]),
        };
    }
}




