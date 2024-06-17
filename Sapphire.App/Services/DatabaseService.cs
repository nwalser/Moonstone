using DeviceId;
using Moonstone.Database;
using Sapphire.Data.AppDb;

namespace Sapphire.App.Services;

public class DatabaseService<TType> where TType : Database, new()
{
    private readonly AppDb _appDb;
    
    public TType? Database { get; private set; }

    public DatabaseService(AppDb appDb)
    {
        _appDb = appDb;
    }

    private string DeviceId => new DeviceIdBuilder()
        .AddMachineName()
        .AddUserName()
        .ToString();

    
    public void Create(string path)
    {
        if (Database is not null) throw new InvalidOperationException();
        
        var db = new TType();
        db.Create(path, DeviceId);
        Database = db;

        _appDb.PushRecent(path);
    }

    public void Open(string path)
    {
        if (Database is not null) throw new InvalidOperationException();

        var db = new TType();
        db.Open(path, DeviceId);
        Database = db;

        _appDb.PushRecent(path);
    }

    public void Close()
    {
        if (Database is null) throw new InvalidOperationException();

        Database.Close();
        Database = null;
    }
}