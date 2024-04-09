using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;
using Newtonsoft.Json;

namespace DistributedSessions;

public class MutationStream
{
    private readonly HashSet<Guid> _mutationIds;
    private readonly SortedList<MutationOccurence, Mutation> _mutations;
    private List<Snapshot> _snapshotCaches;
    
    private readonly ConcurrentQueue<Mutation> _newMutations;
    private readonly ConcurrentQueue<Snapshot> _newSnapshot;

    private readonly string _storagePath;
    
    
    public MutationStream(string storagePath, ConcurrentQueue<Mutation> newMutations,
        ConcurrentQueue<Snapshot> newSnapshot)
    {
        _storagePath = storagePath;
        _newMutations = newMutations;
        _newSnapshot = newSnapshot;

        _mutations = new SortedList<MutationOccurence, Mutation>();
        _mutationIds = new HashSet<Guid>();
        _snapshotCaches = new List<Snapshot>();
    }

    public async Task ExecuteAsync(CancellationToken ct)
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
        
        var firstRun = true;
        while (!ct.IsCancellationRequested)
        {
            var eventsIngested = IngestNewMutations(ct);

            if(!eventsIngested & !firstRun)
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
            
            firstRun = false;
            
            await Task.Delay(10, ct);
        }
    }
    
    public static void Serialize(object value, Stream s)
    {
        using (StreamWriter writer = new StreamWriter(s))
        using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
        {
            JsonSerializer ser = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.All,
            };
            ser.Serialize(jsonWriter, value);
            jsonWriter.Flush();
        }
    }

    public static T Deserialize<T>(Stream s)
    {
        using (StreamReader reader = new StreamReader(s))
        using (JsonTextReader jsonReader = new JsonTextReader(reader))
        {
            JsonSerializer ser = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return ser.Deserialize<T>(jsonReader);
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