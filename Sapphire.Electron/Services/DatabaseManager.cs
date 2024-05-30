using System.Reactive.Subjects;
using System.Text.Json;
using Moonstone.Database;
using Moonstone.Database.Exceptions;

namespace Sapphire.Electron.Services;

public class DatabaseManager<TType> where TType : Database, new()
{ 
    private readonly string _session;
    private readonly List<TType> _databases = [];
    
    private readonly BehaviorSubject<DateTime> _lastUpdate = new(DateTime.MinValue);
    public BehaviorSubject<DateTime> LastUpdate => _lastUpdate;
    
    public DatabaseManager(string openDatabasesFile, string deviceId)
    {
        _session = Math.Abs(deviceId.GetHashCode()).ToString();

        // open existing databases
        if (!File.Exists(openDatabasesFile)) 
            return;
        
        var json = File.ReadAllText(openDatabasesFile);
        var openDatabases = JsonSerializer.Deserialize<string[]>(json) ?? [];

        foreach (var openDatabase in openDatabases)
            Open(openDatabase);
    }

    public TType Find(long id)
    {
        return _databases.Single(d => d.Id == id);
    }

    public IEnumerable<TType> Enumerate()
    {
        return _databases;
    }

    public TType Create(string path)
    {
        var database = new TType();
        database.Create(path, _session);

        Add(database);
        return database;
    }
    
    public TType Open(string path)
    {
        var database = new TType();
        database.Open(path, _session);

        Add(database);
        return database;
    }

    private void Add(TType database)
    {
        if (_databases.Any(d => d.Id == database.Id)) 
            throw new DatabaseAlreadyOpenedException();

        _databases.Add(database);
        _lastUpdate.OnNext(DateTime.UtcNow);
    }

    public void Close(long id)
    {
        var database = Find(id);
        database.Close();
        _databases.Remove(database);
        
        _lastUpdate.OnNext(DateTime.UtcNow);
    }
}