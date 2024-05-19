namespace Amber.Domain.Documents.Project;

public class ProjectAggregate
{
    public string Name { get; set; } = "Your Project Name";
    public List<Guid> Todos { get; set; } = [];
}