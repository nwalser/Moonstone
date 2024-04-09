using DistributedSessions.Mutations;
using Newtonsoft.Json;

namespace DistributedSessions.Projection;

public class Snapshot
{
    public required TimeSpan TargetAge { get; set; }
    public required MutationOccurence LastMutationOccurence { get; set; }

    public required ProjectionModel Model { get; set; }
    
    
    public static Snapshot Create(TimeSpan targetAge)
    {
        return new Snapshot()
        {
            Model = ProjectionModel.Empty(),
            TargetAge = targetAge,
            LastMutationOccurence = new MutationOccurence()
            {
                Occurence = DateTime.MinValue,
                RandomId = Guid.Empty,
            }
        };
    }
    
    
    public static Snapshot Clone(TimeSpan targetAge, Snapshot snapshot)
    {
        var json = JsonConvert.SerializeObject(snapshot);
        var newSnapshot = JsonConvert.DeserializeObject<Snapshot>(json)!;
        newSnapshot.TargetAge = targetAge;
        return newSnapshot;
    }
    
    
    public void AppendMutation(Mutation mutation)
    {
        if (LastMutationOccurence is not null && LastMutationOccurence >= mutation.Occurence)
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