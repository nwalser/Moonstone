namespace Sapphire.Electron.Services;

public class DatabaseManager<TType>
{ 
    private string _session;

    public DatabaseManager(string session)
    {
        _session = session;
    }

    public TType Open(string path)
    {
        throw new NotImplementedException();
    }

    public void Close(long id)
    {
        throw new NotImplementedException();
    }
}