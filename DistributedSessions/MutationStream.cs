using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;

namespace DistributedSessions;

public class MutationStream(
    HashSet<Guid> mutationIds,
    SortedList<DateTime, Mutation> mutations,
    ConcurrentQueue<Mutation> newMutations,
    ConcurrentQueue<Mutation> newMutations2,
    Dictionary<TimeSpan, Snapshot> snapshots1,
    ConcurrentQueue<Snapshot> newSnapshot)
{
    private readonly HashSet<Guid> _mutationIds = mutationIds;
    private readonly SortedList<DateTime, Mutation> _mutations = mutations;
    private readonly Dictionary<TimeSpan, Snapshot> _snapshots = snapshots1;

    private readonly ConcurrentQueue<Mutation> _newMutations = newMutations;
    private readonly ConcurrentQueue<Mutation> _newMutations2 = newMutations2;
    private readonly ConcurrentQueue<Snapshot> _newSnapshot = newSnapshot;

    private Snapshot _snapshot = Snapshot.Create();
    
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var t1 = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                await IngestNewMutations(ct);
                await Task.Delay(10, ct);
            }
        }, ct);

        var t2 = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                RebuildSnapshots(ct);
                await Task.Delay(10, ct);
            }
        }, ct);

        await Task.WhenAll(t1, t2);
    }
    
    private Task IngestNewMutations(CancellationToken ct)
    {
        while (_newMutations.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
        {
            if (_mutationIds.Contains(mutation.MutationId))
                continue;
            
            _mutations.Add(mutation.Occurence, mutation);
            _mutationIds.Add(mutation.MutationId);
            
            _newMutations2.Enqueue(mutation);
        }

        return Task.CompletedTask;
    }
    
    private void RebuildSnapshots(CancellationToken ct)
    {
        while (_newMutations2.TryDequeue(out var mutation))
        {
            Console.WriteLine(_newMutations2.Count);
            if (mutation.Occurence > _snapshot.SnapshotTime)
            {
                _snapshot.AppendMutation(mutation);
            }
            else
            {
                _snapshot = GetSnapshot();
            }
            _newSnapshot.Enqueue(_snapshot);
        }
    }

    private Snapshot GetSnapshot()
    {
        var snapshot = Snapshot.Create();

        foreach (var (occurence, mutation) in _mutations)
        {
            snapshot.AppendMutation(mutation);
        }

        return snapshot;
    }
}