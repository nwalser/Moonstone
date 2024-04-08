using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;

namespace DistributedSessions;

public class MutationStream(
    HashSet<Guid> mutationIds,
    SortedList<DateTime, Mutation> mutations,
    ConcurrentQueue<Mutation> newMutations,
    Dictionary<TimeSpan, Snapshot> snapshots1)
{
    private readonly HashSet<Guid> _mutationIds = mutationIds;
    private readonly SortedList<DateTime, Mutation> _mutations = mutations;
    private readonly Dictionary<TimeSpan, Snapshot> _snapshots = snapshots1;

    private readonly ConcurrentQueue<Mutation> _newMutations = newMutations;

    private static readonly List<TimeSpan> SnapshotAges =
    [
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromSeconds(10)
    ];
    
    public async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                await IngestNewMutations(ct);
                await Task.Delay(10, ct);
            }
        }, ct);

        await Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                RebuildSnapshots(ct);
                await Task.Delay(10, ct);
            }
        }, ct);
    }
    
    private Task IngestNewMutations(CancellationToken ct)
    {
        while (_newMutations.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
        {
            if (_mutationIds.Contains(mutation.MutationId))
                continue;
            
            _mutations.Add(mutation.Occurence, mutation);
            _mutationIds.Add(mutation.MutationId);

            // remove stale snapshots
            lock (_snapshots)
            {
                foreach(var toRemove in _snapshots.Where(s => s.Value.SnapshotTime > mutation.Occurence).ToList())
                {
                    _snapshots.Remove(toRemove.Key);
                }
            }
        }

        return Task.CompletedTask;
    }
    
    private void RebuildSnapshots(CancellationToken ct)
    {
        // move forward to target age
        foreach (var snapshotAge in SnapshotAges.OrderByDescending(a => a))
        {
            var newSnapshot = GetSnapshot();
            lock (_snapshots)
            {
                _snapshots.Remove(snapshotAge);
                _snapshots.Add(snapshotAge, newSnapshot);
            }
        }
    }

    private Snapshot GetSnapshot()
    {
        var snapshot = Snapshot.Create(TimeSpan.Zero);

        foreach (var (occurence, mutation) in _mutations)
        {
            snapshot.AppendMutation(mutation);
        }

        return snapshot;
    }
}