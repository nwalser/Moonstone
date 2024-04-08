using System.Collections;

namespace DistributedSessions;

public class MutationStream
{
    private HashSet<Guid> _mutationIds { get; set; }
    private SortedList<DateOnly, Mutation> _mutations;
}