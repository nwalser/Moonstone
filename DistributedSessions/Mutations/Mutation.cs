using ProtoBuf;

namespace DistributedSessions.Mutations;

public abstract class Mutation
{
    public required Guid Id { get; set; }
    public required DateTime Occurence { get; set; }
}