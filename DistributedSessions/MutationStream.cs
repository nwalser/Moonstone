using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;

namespace DistributedSessions;

public class MutationStream(
    HashSet<Guid> mutationIds,
    SortedList<MutationOccurence, Mutation> mutations,
    ConcurrentQueue<Mutation> newMutations,
    ConcurrentQueue<Snapshot> newSnapshot,
    List<Snapshot> snapshotCaches)
{
    private readonly HashSet<Guid> _mutationIds = mutationIds;
    private readonly SortedList<MutationOccurence, Mutation> _mutations = mutations;
    private readonly List<Snapshot> _snapshotCaches = snapshotCaches;

    private readonly ConcurrentQueue<Mutation> _newMutations = newMutations;
    private readonly ConcurrentQueue<Snapshot> _newSnapshot = newSnapshot;

    private readonly object _streamState = new();
    
    public async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var eventsIngested = IngestNewMutations(ct);

            if (eventsIngested)
                RebuildSnapshots(ct);
            
            await Task.Delay(10, ct);
        }
    }
    
    private bool IngestNewMutations(CancellationToken ct)
    {
        var eventsIngested = false;
        
        try
        {
            while (_newMutations.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
            {
                if (_mutationIds.Contains(mutation.MutationId))
                    continue;

                _mutations.Add(mutation.Occurence, mutation);
                _mutationIds.Add(mutation.MutationId);

                // invalidate caches
                var invalidCaches = _snapshotCaches
                    .Where(s => s.LastMutationOccurence >= mutation.Occurence)
                    .ToList();

                foreach (var invalidCache in invalidCaches)
                    _snapshotCaches.Remove(invalidCache);
                
                eventsIngested = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        
        return eventsIngested;
    }
    
    private void RebuildSnapshots(CancellationToken ct)
    {
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
                        .Where(s => s.LastMutationOccurence?.Occurence < targetDate)
                        .MaxBy(v => v.LastMutationOccurence?.Occurence);

                    if (bestParent is null)
                        bestParent = Snapshot.Create(wantedSnapshotAge);

                    // if parent snapshot is not old self create a copy
                    if (bestParent.TargetAge != wantedSnapshotAge)
                        bestParent = Snapshot.Clone(wantedSnapshotAge, bestParent);

                    // apply remaining mutations on top of it
                    var remainingMutations = _mutations
                        .Where(m => m.Key > bestParent.LastMutationOccurence)
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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}