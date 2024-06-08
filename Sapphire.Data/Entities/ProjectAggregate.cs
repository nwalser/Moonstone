using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class ProjectAggregate : Document
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; } = 0;
    public DateOnly Start { get; set; }
    public DateOnly? Deadline { get; set; }

    public List<string> PossibleTags { get; set; } = [];


    public IEnumerable<TodoAggregate> GetRootTodos(ProjectDatabase db)
    {
        return db.Enumerate<TodoAggregate>()
            .Where(t => t.ProjectId == Id)
            .Where(t => t.ParentId is null);
    }

    public void Delete(ProjectDatabase db)
    {
        // todo: delete all related entities
        db.Remove(this);
    }
}