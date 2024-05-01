using Moonstone.Workspace.MutationStream;

namespace Moonstone.Domain.Mutations.Project.ChangeName;

public class ChangeProjectName : IMutation
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
}