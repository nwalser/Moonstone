using DistributedSessions.Mutations;

namespace DistributedSessions.Projection;

public class Snapshot
{
    public required TimeSpan TargetAge { get; set; }
    public required DateTime SnapshotTime { get; set; }
    public required ProjectionModel Model { get; set; }


    public static Snapshot Create(TimeSpan targetAge)
    {
        return new Snapshot()
        {
            TargetAge = targetAge,
            SnapshotTime = DateTime.MinValue,
            Model = ProjectionModel.Empty(),
        };
    }
    
    
    public void AppendMutation(Mutation mutation)
    {
        if (mutation.Occurence <= SnapshotTime)
            throw new InvalidOperationException("Mutation is not applied in order");
        
        SnapshotTime = mutation.Occurence;

        switch (mutation)
        {
            case CreateProjectMutation createProject:
                Model.CreatedProjects++;
                break;
        }
    }
}