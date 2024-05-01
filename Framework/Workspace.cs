using RT.Comb;

namespace Framework;

public class Workspace<TProjection>
{
    public required PostgreSqlCombProvider Comb { get; set; }
    public required RollingMutationStream MutationStream { get; set; }
    

    public static Workspace<TProjection> InitializeFrom(PathProvider paths, CancellationToken ct = default)
    {
        var stream = RollingMutationStream.InitializeFrom(paths.GetSessionMutationsFolder(), ct);
        
        return new Workspace<TProjection>()
        {
            Comb = new PostgreSqlCombProvider(new SqlDateTimeStrategy()),
            MutationStream = stream
        };
    }

    public void AppendMutation(IMutation mutation)
    {
        var entry = new MutationLogEntry()
        {
            Id = Comb.Create(),
            Mutation = mutation
        };
        
        MutationStream.Append(entry);
    }

    private static string GetSessionFolder(string workspaceFolder) => Path.Join(workspaceFolder, "mutations");
}