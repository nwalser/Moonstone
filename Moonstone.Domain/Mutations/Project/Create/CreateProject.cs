using MessagePack;
using Moonstone.Workspace.Data;

namespace Moonstone.Domain.Mutations.Project.Create;


[MessagePackObject]
public class CreateProject : IMutation
{    
    [Key(0)] public required Guid ProjectId { get; set; }
    [Key(1)] public required string Name { get; set; }
}