using Moonstone.Framework.Stream;

namespace Moonstone.Framework.Mutations;

public class DeleteProjectMutation : Mutation
{
    public required Guid ProjectId { get; set; }
}