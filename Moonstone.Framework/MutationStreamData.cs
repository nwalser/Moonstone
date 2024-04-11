using Moonstone.Framework.Mutations;
using Moonstone.Framework.Projection;

namespace Moonstone.Framework;

public class MutationStreamData
{
    public required List<Mutation> Mutations { get; set; }
    public required List<Snapshot> SnapshotCaches { get; set; }
}