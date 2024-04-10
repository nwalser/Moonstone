namespace DistributedSessions;

public class PathProvider
{
    public required string Workspace { get; set; }
    public required Guid Session { get; set; }
    public required string Temporary { get; set; }

    public string GetSessionPath()
    {
        return Path.Join(Workspace, Session.ToString());
    }

    public string GetSessionMutationsFolder()
    {
        return Path.Join(GetSessionPath(),  "mutations");
    }

    public string GetSessionMutationsFile(int fileNumber)
    {
        return Path.Join(GetSessionMutationsFolder(), $"{fileNumber}.nljson");
    }
}