namespace Moonstone.Framework;

public class PathProvider
{
    public required string Workspace { get; set; }
    public required string Session { get; set; }
    public required string Temporary { get; set; }

    public string GetSessionPath()
    {
        return Path.Join(Workspace, Session);
    }

    public string GetSessionMutationsFolder()
    {
        return Path.Join(GetSessionPath(),  "mutations");
    }

    public string GetSessionMutationsFile(int fileNumber)
    {
        return Path.Join(GetSessionMutationsFolder(), $"{fileNumber}.nljson");
    }
    
    public string GetStreamStoreFolder()
    {
        return Path.Join(Temporary, "stream");
    }
    
    public string GetStreamStoreDbFile()
    {
        return Path.Join(GetStreamStoreFolder(), "sqlite.db");
    }
}