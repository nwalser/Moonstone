using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using Stream.Mutations;

namespace Stream.FileStore;

public class MutationStream
{
    private readonly MutationStreamStore _store;

    private readonly IObservable<Mutation> _externalMutation;
    private readonly Subject<Mutation> _persistMutation;
    private readonly Subject<Mutation> _mutationCached;


    public MutationStream(IObservable<Mutation> externalMutation,
        Subject<Mutation> persistMutation, Subject<Mutation> mutationCached,
        MutationStreamStore streamStore)
    {
        _externalMutation = externalMutation;
        _persistMutation = persistMutation;
        _mutationCached = mutationCached;
        _store = streamStore;

        _externalMutation.Subscribe(ExternalUpdate);
    }


    public void AddMutation(Mutation mutation)
    {
        CacheMutation(mutation);
        _persistMutation.OnNext(mutation);
        _mutationCached.OnNext(mutation);
    }

    public IEnumerable<Mutation> ReplaySince(DateTime after)
    {
        return _store.CachedMutations
            .Where(c => c.Occurence >= after)
            .OrderBy(c => c.Occurence)
            .Select(m => MapFrom(m))
            .AsEnumerable();
    }


    private void ExternalUpdate(Mutation mutation)
    {
        lock (_store)
        {
            var mutationExists = _store.CachedMutations.Any(m => m.MutationId == mutation.MutationId);
            if (mutationExists) return;
        }
        
        CacheMutation(mutation);
        _mutationCached.OnNext(mutation);
    }

    private void CacheMutation(Mutation mutation)
    {
        var cachedMutation = MapFrom(mutation);

        lock (_store)
        {
            _store.CachedMutations.Add(cachedMutation);
            _store.SaveChanges();
        }
    }

    private static Mutation MapFrom(CachedMutation mutation)
    {
        return MutationSerializer.Deserialize(mutation.MutationJson);
    }

    private static CachedMutation MapFrom(Mutation mutation)
    {
        return new CachedMutation
        {
            Occurence = mutation.Occurence,
            MutationId = mutation.MutationId,
            MutationJson = MutationSerializer.Serialize(mutation)
        };
    }
}