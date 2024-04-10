using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DistributedSessions;

public class MutationStream : BackgroundWorker<MutationStream>
{
    private readonly HashSet<Guid> _mutationIds;
    private readonly SortedList<MutationOccurence, Mutation> _mutations;
    private List<Snapshot> _snapshotCaches;
    
    private readonly ConcurrentQueue<Mutation> _newMutations;
    private readonly ConcurrentQueue<Snapshot> _newSnapshot;

    private readonly PathProvider _paths;
    
    
    public MutationStream(ConcurrentQueue<Mutation> newMutations, ConcurrentQueue<Snapshot> newSnapshot, PathProvider paths, CancellationToken ct, ILogger<MutationStream> logger) : base(ct, logger)
    {
        _newMutations = newMutations;
        _newSnapshot = newSnapshot;
        _paths = paths;

        _mutations = new SortedList<MutationOccurence, Mutation>();
        _mutationIds = new HashSet<Guid>();
        _snapshotCaches = new List<Snapshot>();
    }

    
    protected override async Task Initialize(CancellationToken ct)
    {
        // load from file system
        var tempFilePath = Path.Join(_storagePath, "temp.json");
        if (File.Exists(tempFilePath))
        {
            try
            {
                await using var stream = File.OpenRead(tempFilePath);
                var data = Deserialize<MutationStreamData>(stream);

                _snapshotCaches = data.SnapshotCaches;
                
                foreach (var mutation in data.Mutations)
                {
                    _mutations.Add(mutation.Occurence, mutation);
                    _mutationIds.Add(mutation.MutationId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    
    protected override async Task ProcessWork(CancellationToken ct)
    {
        var eventsIngested = IngestNewMutations(ct);

        if(!eventsIngested)
            continue;
            
        RebuildSnapshots(ct);

        Directory.CreateDirectory(_storagePath);
        await using (var stream = File.OpenWrite(tempFilePath))
        {
            var data = new MutationStreamData()
            {
                Mutations = _mutations.Select(m => m.Value).ToList(),
                SnapshotCaches = _snapshotCaches,
            };
                
            Serialize(data, stream);
        }
            
        await Task.Delay(10, ct);
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}