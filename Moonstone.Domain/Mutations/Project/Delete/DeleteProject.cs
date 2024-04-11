using Moonstone.Framework.Stream;

namespace Moonstone.Domain.Mutations.Project.Delete;

public class DeleteProject : Mutation
{
    public required Guid ProjectId { get; set; }
}