using System.Text.Json.Serialization;

namespace Sapphire.Data.Entities.Todo;

public partial class TodoAggregate
{
    [JsonInclude] public List<Guid>? PossibleWorkerIds { get; set; }

    [JsonIgnore]
    public bool InheritsPossibleWorkerIds
    {
        get => PossibleWorkerIds is null;
        set => PossibleWorkerIds = value ? null : [];
    }

    public void RemovePossibleWorkerIds(Guid workerId) => PossibleWorkerIds?.Remove(workerId);
    public void AddPossibleWorkerIds(Guid workerId) => PossibleWorkerIds?.Add(workerId);

    public void SetPossibleWorkerIds(List<Guid> workerIds)
    {
        if (!InheritsPossibleWorkerIds)
            PossibleWorkerIds = workerIds;
    }

    public IEnumerable<Guid> GetPossibleWorkerIds(ProjectDatabase db)
    {
        if (PossibleWorkerIds is not null)
            return PossibleWorkerIds;
        
        return GetParentTodo(db)?.GetPossibleWorkerIds(db) ?? [];
    }

    public IEnumerable<WorkerAggregate> GetPossibleWorkers(ProjectDatabase db)
    {
        return db.Enumerate<WorkerAggregate>()
            .Where(w => GetPossibleWorkerIds(db).Contains(w.Id));
    }
}