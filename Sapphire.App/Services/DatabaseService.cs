using System.Text.Json;
using DeviceId;
using Moonstone.Database;

namespace Sapphire.App.Services;

public class DatabaseService<TType> where TType : Database, new()
{
    public TType? Database { get; private set; }

    private readonly List<string> _recentDatabases = [];
    public IReadOnlyList<string> RecentDatabases => _recentDatabases;
    private readonly string _recentDatabaseStoragePath;

    public DatabaseService(string recentDatabaseStoragePath)
    {
        _recentDatabaseStoragePath = recentDatabaseStoragePath;

        if (File.Exists(_recentDatabaseStoragePath))
        {
            var json = File.ReadAllText(_recentDatabaseStoragePath);
            _recentDatabases = JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
    }

    private string DeviceId => new DeviceIdBuilder()
        .AddMachineName()
        .AddMacAddress()
        .AddUserName()
        .ToString();

    public void RemoveRecent(string path)
    {
        _recentDatabases.Remove(path);
    }
    
    private void PushRecent(string path)
    {
        RemoveRecent(path);
        _recentDatabases.Add(path);
    }

    private void StoreRecent()
    {
        var json = JsonSerializer.Serialize(RecentDatabases);
        File.WriteAllText(_recentDatabaseStoragePath, json);
    }
    
    public void Create(string path)
    {
        if (Database is not null) throw new InvalidOperationException();
        
        var db = new TType();
        db.Create(path, DeviceId);
        Database = db;

        PushRecent(path);
    }

    public void Open(string path)
    {
        if (Database is not null) throw new InvalidOperationException();

        var db = new TType();
        db.Open(path, DeviceId);
        Database = db;

        PushRecent(path);
    }

    public void Close()
    {
        if (Database is null) throw new InvalidOperationException();

        Database.Close();
        Database = null;
    }
}