using DistributedSessions.Mutations;
using DistributedSessions.Projection;

namespace DistributedSessions;

public class MutationStreamData
{
    public required List<Mutation> Mutations { get; set; }
    public required List<Snapshot> SnapshotCaches { get; set; }
}