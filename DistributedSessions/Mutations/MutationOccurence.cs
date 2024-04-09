using ProtoBuf;

namespace DistributedSessions.Mutations;

[ProtoContract]
public record MutationOccurence : IComparable<MutationOccurence>
{
    [ProtoMember(1)]
    public required Guid RandomId { get; set; }
    [ProtoMember(2)]
    public required DateTime Occurence { get; set; }
    
    
    public int CompareTo(MutationOccurence? other)
    {
        if (other?.Occurence != Occurence)
            return Occurence.CompareTo(other?.Occurence);

        return RandomId.CompareTo(other.RandomId);
    }
    
    public static bool operator  < (MutationOccurence x, MutationOccurence y) { return x.CompareTo(y)  < 0; }
    public static bool operator  > (MutationOccurence x, MutationOccurence y) { return x.CompareTo(y)  > 0; }
    public static bool operator <= (MutationOccurence x, MutationOccurence y) { return x.CompareTo(y) <= 0; }
    public static bool operator >= (MutationOccurence x, MutationOccurence y) { return x.CompareTo(y) >= 0; }
}