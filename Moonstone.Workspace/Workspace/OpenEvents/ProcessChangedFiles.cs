namespace Moonstone.Workspace.Workspace.OpenEvents;

public class ProcessChangedFiles : WorkspaceEvent
{
    public required int Current { get; init; }
    public required int Total { get; init; }
}