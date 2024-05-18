namespace Amber.Domain.Documents.Project;

public class Project
{
    public required string Name { get; set; }
    public required int Counter { get; set; }


    public void ChangeProjectName(string projectName)
    {
        Name = projectName;
    }

    public void IncreaseCounter(int increment)
    {
        Counter += increment;
    }
    
    
    public override string ToString()
    {
        return $"{Name}, {Counter}";
    }
}