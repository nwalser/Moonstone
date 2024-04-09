using DistributedSessions.Mutations;
using Newtonsoft.Json;

namespace DistributedSessions.Projection;

public class Snapshot
{
    public required TimeSpan TargetAge { get; set; }
    public required DateTime LastMutationTime { get; set; }

    private Mutation? _lastMutation;
    
    public required ProjectionModel Model { get; set; }
    
    
    public static Snapshot Create(TimeSpan targetAge)
    {
        return new Snapshot()
        {
            Model = ProjectionModel.Empty(),
            TargetAge = targetAge,
            LastMutationTime = DateTime.MinValue
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
        if(_lastMutation?.CompareTo(mutation) > 0)
            throw new InvalidOperationException("Mutation is not applied in order");

        _lastMutation = mutation;
        LastMutationTime = mutation.Occurence;

        switch (mutation)
        {
            case CreateProjectMutation createProject:
                Model.CreatedProjects++;
                break;
        }
    }
}