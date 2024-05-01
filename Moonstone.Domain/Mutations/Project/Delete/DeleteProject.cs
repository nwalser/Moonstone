using Moonstone.Workspace.MutationStream;

namespace Moonstone.Domain.Mutations.Project.Delete;

public class DeleteProject : IMutation
{
    public required Guid ProjectId { get; set; }
}