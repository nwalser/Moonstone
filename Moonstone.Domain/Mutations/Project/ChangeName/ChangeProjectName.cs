using Moonstone.Framework.Stream;

namespace Moonstone.Domain.Mutations.Project.ChangeName;

public class ChangeProjectName : Mutation
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
}