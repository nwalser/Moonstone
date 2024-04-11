using Moonstone.Framework.Stream;

namespace Moonstone.Domain.Mutations.Project.Delete;

public class DeleteProjectMutation : Mutation
{
    public required Guid ProjectId { get; set; }
}