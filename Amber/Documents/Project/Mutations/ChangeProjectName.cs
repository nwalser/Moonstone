﻿namespace Amber.Documents.Project.Mutations;

public record ChangeProjectName
{
    public required string Name { get; init; }
}