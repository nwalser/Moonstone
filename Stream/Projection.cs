using Stream.Mutations.Project.ChangeProjectName;
using Stream.Mutations.Project.CreateProject;
using Stream.Mutations.Project.DeleteProject;

namespace Stream;

public class Projection
{
    public required List<Project> Projects { get; set; }
}