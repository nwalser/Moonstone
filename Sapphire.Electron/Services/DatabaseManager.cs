using DeviceId;
using Moonstone.Database;
using Sapphire.Data;

namespace Sapphire.Electron.Services;

public class DatabaseManager<TType> where TType : Database, new()
{
    public TType? Database { get; private set; }


    public void Create(string path)
    {
        if (Database is not null) throw new InvalidOperationException();
        
        var deviceId = new DeviceIdBuilder()
            .AddMachineName()
            .AddMacAddress()
            .AddUserName()
            .ToString();
        var db = new TType();
        db.Create(path, deviceId);

        Database = db;
    }

    public void Open(string path)
    {
        if (Database is not null) throw new InvalidOperationException();

        var deviceId = new DeviceIdBuilder()
            .AddMachineName()
            .AddMacAddress()
            .AddUserName()
            .ToString();
        var db = new TType();
        db.Open(path, deviceId);

        Database = db;
    }

    public void Close(string path)
    {
        if (Database is null) throw new InvalidOperationException();

        Database.Close();
        Database = null;
    }
}