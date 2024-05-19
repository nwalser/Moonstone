using System.Text.Json;
using Amber.Domain.Documents;
using Moonstone;

namespace Amber.App.Services;

public class WorkspaceManager
{
    private readonly string _storePath;

    public Dictionary<int, AmberWorkspace> Workspaces { get; set; } = [];

    public WorkspaceManager(string storePath)
    {
        _storePath = storePath;
        LoadOpenWorkspaces();
    }
    
    public Workspace Open(string path)
    {
        var workspace = new AmberWorkspace(path);
        Workspaces.Add(path.GetHashCode(), workspace);

        SaveOpenWorkspaces();
        
        return workspace;
    }

    public void Close(int workspaceId)
    {
        var workspace = Workspaces[workspaceId];
        Workspaces.Remove(workspaceId);
        
        SaveOpenWorkspaces();
    }

    private void SaveOpenWorkspaces()
    {
        var json = JsonSerializer.Serialize(Workspaces.Select(w => w.Value.Location));
        File.WriteAllText(_storePath, json);
    }

    private void LoadOpenWorkspaces()
    {
        var openWorkspaces = (File.Exists(_storePath)
            ? JsonSerializer.Deserialize<string[]>(File.ReadAllText(_storePath))
            : []) ?? throw new InvalidOperationException();
        
        Workspaces = openWorkspaces.ToDictionary(w => w.GetHashCode(), w => new AmberWorkspace(w));
    }
}