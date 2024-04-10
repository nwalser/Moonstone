using DistributedSessions.Mutations;
using Newtonsoft.Json;

namespace DistributedSessions.Projection;

public class Snapshot
{
    public required Mutation LastMutation { get; set; }
    public required ProjectionModel Model { get; set; }
    
    
    public static Snapshot Create()
    {
        return new Snapshot()
        {
            Model = ProjectionModel.Empty(),
            LastMutation = new StartStreamMutation()
            {
                Id = Guid.Empty,
                Occurence = DateTime.MinValue,
            }
        };
    }
    
    public void AppendMutation(Mutation mutation)
    {
        if (LastMutation?.Occurence > mutation.Occurence)
            throw new InvalidOperationException("Mutation is not applied in order");

        if (LastMutation?.Occurence == mutation.Occurence && LastMutation.Id > mutation.Id)
            throw new InvalidOperationException("Mutation is not applied in order");

        LastMutation = mutation;

        switch (mutation)
        {
            case CreateProjectMutation createProject:
                Model.CreatedProjects++;
                break;
            case DeleteProjectMutation deleteProject:
                Model.CreatedProjects++;
                break;
        }
    }
}