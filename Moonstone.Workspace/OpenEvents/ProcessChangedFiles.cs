namespace Moonstone.Workspace.OpenEvents;

public class ProcessChangedFiles : WorkspaceEvent
{
    public required int Current { get; init; }
    public required int Total { get; init; }
}