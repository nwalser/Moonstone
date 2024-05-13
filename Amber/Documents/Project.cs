namespace Amber.Documents;

public class Project
{
    public string Name { get; set; } = string.Empty;
    public int Counter { get; set; } = 0;
}

public record ChangeProjectName
{
    public required string Name { get; init; }
}

public record IncreaseCounter
{
    public required int Count { get; init; }
}