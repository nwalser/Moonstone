using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;

namespace DistributedSessions;

public class MutationStream(
    HashSet<Guid> mutationIds,
    SortedList<Mutation, Mutation> mutations,
    ConcurrentQueue<Mutation> newMutations,
    ConcurrentQueue<Snapshot> newSnapshot,
    List<Snapshot> snapshotCaches)
{
    private readonly HashSet<Guid> _mutationIds = mutationIds;
    private readonly SortedList<Mutation, Mutation> _mutations = mutations;
    private readonly List<Snapshot> _snapshotCaches = snapshotCaches;

    private readonly ConcurrentQueue<Mutation> _newMutations = newMutations;
    private readonly ConcurrentQueue<Snapshot> _newSnapshot = newSnapshot;

    private readonly object _streamState = new();
    private bool _snapshotUpdateNeeded = false;
    
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
        try
        {
            lock (_streamState)
            {
                while (_newMutations.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
                {
                    if (_mutationIds.Contains(mutation.MutationId))
                        continue;

                    _mutations.Add(mutation, mutation);
                    _mutationIds.Add(mutation.MutationId);

                    // invalidate caches
                    var invalidCaches = _snapshotCaches
                        .Where(s => s.LastMutationTime >= mutation.Occurence)
                        .ToList();

                    foreach (var invalidCache in invalidCaches)
                        _snapshotCaches.Remove(invalidCache);
                    
                    _snapshotUpdateNeeded = true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        
        return Task.CompletedTask;
    }
    
    private void RebuildSnapshots(CancellationToken ct)
    {
        // do not start processing snapshots if any mutations are not ingested
        if (!_snapshotUpdateNeeded)
            return;

        try
        {
            lock (_streamState)
            {
                var wantedSnapshotAges = new List<TimeSpan>()
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromMinutes(10),
                };

                var now = DateTime.UtcNow;

                foreach (var wantedSnapshotAge in wantedSnapshotAges.OrderDescending())
                {
                    var targetDate = now - wantedSnapshotAge;

                    var bestParent = _snapshotCaches
                        .Where(s => s.LastMutationTime < targetDate)
                        .MaxBy(v => v.LastMutationTime);

                    if (bestParent is null)
                        bestParent = Snapshot.Create(wantedSnapshotAge);

                    // if parent snapshot is not old self create a copy
                    if (bestParent.TargetAge != wantedSnapshotAge)
                        bestParent = Snapshot.Clone(wantedSnapshotAge, bestParent);

                    // apply remaining mutations on top of it
                    var remainingMutations = _mutations
                        .Where(m => m.Key.Occurence >= bestParent.LastMutationTime)
                        .Where(m => m.Key.Occurence <= targetDate);

                    foreach (var (occurence, mutation) in remainingMutations)
                        bestParent.AppendMutation(mutation);


                    var snapshotToReplace = _snapshotCaches.SingleOrDefault(s => s.TargetAge == wantedSnapshotAge);
                    if (snapshotToReplace is not null)
                        _snapshotCaches.Remove(snapshotToReplace);

                    _snapshotCaches.Add(bestParent);

                    if (wantedSnapshotAge == TimeSpan.Zero)
                        _newSnapshot.Enqueue(bestParent);
                }

                _snapshotUpdateNeeded = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public class MutationOccurence : IComparable<MutationOccurence>
{
    public required Guid MutationId { get; set; }
    public required DateTime Occurence { get; set; }
    
    public int CompareTo(MutationOccurence? other)
    {
        throw new NotImplementedException();
    }
}