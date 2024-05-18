namespace Amber.Domain.Documents.Project;

public class Project
{
    public required string Name { get; set; }
    
    public DateOnly? Deadline { get; set; }


    public void ChangeProjectName(string projectName)
    {
        Name = projectName;
    }
}