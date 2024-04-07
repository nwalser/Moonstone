using Stream.Mutations.Project.ChangeProjectName;

namespace Stream;

public class Project
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    
    
    public void ChangeName(string name)
    {
        Name = name;
    }
}