using DistributedSessions.Mutations;
using Newtonsoft.Json;

namespace DistributedSessions.Projection;

public class Snapshot
{
    public required DateTime LastMutationOccurence { get; set; }
    public required ProjectionModel Model { get; set; }
    
    
    public static Snapshot Create()
    {
        return new Snapshot()
        {
            Model = ProjectionModel.Empty(),
            LastMutationOccurence = DateTime.MinValue,
        };
    }
    
    public void AppendMutation(Mutation mutation)
    {
        if (LastMutationOccurence > mutation.Occurence)
            throw new InvalidOperationException("Mutation is not applied in order");

        LastMutationOccurence = mutation.Occurence;

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