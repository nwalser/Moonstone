namespace DistributedSessions;

public static class PathFactory
{
    public static string GetSessionPath(string workspace, Guid sessionId)
    {
        return Path.Join(workspace, sessionId.ToString());
    }

    public static string GetSessionMutationsFolder(string workspace, Guid sessionId)
    {
        return Path.Join(GetSessionPath(workspace, sessionId),  "mutations");
    }

    public static string GetSessionMutationsFile(string workspace, Guid sessionId, int fileNumber)
    {
        return Path.Join(GetSessionMutationsFolder(workspace, sessionId), $"{fileNumber}.nljson");
    }
}