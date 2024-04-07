using System.Reactive.Subjects;
using Newtonsoft.Json;
using Stream.Mutations;

namespace Client1;

public class MutationStream
{
    private readonly MutationCache _cache;
    
    private readonly IObservable<Mutation> _mutationAddedExternally;
    private readonly Subject<Mutation> _persistMutation;
    private readonly Subject<Mutation> _mutationCached;
    
    
    public MutationStream(MutationCache cache, IObservable<Mutation> mutationAddedExternally, Subject<Mutation> persistMutation, Subject<Mutation> mutationCached)
    {
        _cache = cache;
        _mutationAddedExternally = mutationAddedExternally;
        _persistMutation = persistMutation;
        _mutationCached = mutationCached;

        _mutationAddedExternally.Subscribe(ExternalUpdate);
    }

    
    public void AddMutation(Mutation mutation)
    {
        CacheMutation(mutation);
        _persistMutation.OnNext(mutation);
        _mutationCached.OnNext(mutation);
    }
    
    public IEnumerable<Mutation> ReplaySince(DateTime after)
    {
        return _cache.CachedMutations
            .Where(c => c.Occurence >= after)
            .OrderBy(c => c.Occurence)
            .Select(m => MapFrom(m))
            .AsEnumerable();
    }
    
    
    private void ExternalUpdate(Mutation mutation)
    {
        var existingMutation = _cache.CachedMutations.Any(m => m.MutationId == mutation.MutationId);
        if (existingMutation) 
            return;

        CacheMutation(mutation);
        _mutationCached.OnNext(mutation);
    }

    private void CacheMutation(Mutation mutation)
    {
        var cachedMutation = MapFrom(mutation);
        _cache.CachedMutations.Add(cachedMutation);
        _cache.SaveChanges();
    }
    
    
    private static readonly JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private static Mutation MapFrom(CachedMutation mutation)
    {
        return JsonConvert.DeserializeObject<Mutation>(mutation.MutationJson, SerializerSettings)!;
    }

    private static CachedMutation MapFrom(Mutation mutation)
    {
        return new CachedMutation()
        {
            Occurence = mutation.Occurence,
            MutationId = mutation.MutationId,
            MutationJson = JsonConvert.SerializeObject(mutation, SerializerSettings)
        };
    }
}