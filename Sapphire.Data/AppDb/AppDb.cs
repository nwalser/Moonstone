using System.Reactive.Subjects;
using System.Text.Json;

namespace Sapphire.Data.AppDb;

public class AppDb
{
    private readonly string _storagePath;
    public AppDbData Data { get; set; } = new();

    public BehaviorSubject<DateTime> LastUpdate { get; set; } = new(DateTime.MinValue);
    
    public AppDb(string storagePath)
    {
        _storagePath = storagePath;
        
        if (File.Exists(_storagePath))
        {
            var json = File.ReadAllText(_storagePath);
            Data = JsonSerializer.Deserialize<AppDbData>(json) ?? new AppDbData();
        } 
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Data);
        File.WriteAllText(_storagePath, json);
        LastUpdate.OnNext(DateTime.UtcNow);
    }
    
    public void RemoveRecent(string path)
    {
        Data.RecentDatabases.Remove(path);
        Save();
    }
    
    public void PushRecent(string path)
    {
        RemoveRecent(path);
        Data.RecentDatabases.Add(path);
        Save();
    }
}