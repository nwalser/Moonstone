using System.Reactive.Subjects;
using Moonstone.Database;

namespace Sapphire.Electron.Services;

public class DatabaseManager<TType> where TType : Database, new()
{ 
    private readonly string _session;
    private readonly List<TType> _databases = [];
    
    private readonly BehaviorSubject<DateTime> _lastUpdate = new(DateTime.MinValue);
    public BehaviorSubject<DateTime> LastUpdate => _lastUpdate;
    
    public DatabaseManager(string storagePath, string deviceId)
    {
        _session = Math.Abs(deviceId.GetHashCode()).ToString();
    }

    public TType Find(long id)
    {
        return _databases.Single(d => d.Id == id);
    }

    public IEnumerable<TType> Enumerate()
    {
        return _databases;
    }
    
    public TType Open(string path)
    {
        var database = new TType();
        database.Open(path, _session);
        _databases.Add(database);

        return database;
    }

    public void Close(long id)
    {
        var database = Find(id);
        database.Close();
        _databases.Remove(database);
    }
}