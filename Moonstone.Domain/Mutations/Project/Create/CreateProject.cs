using Moonstone.Framework.Stream;

namespace Moonstone.Domain.Mutations.Project.Create;

public class CreateProject : Mutation
{    
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
}