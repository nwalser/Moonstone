﻿namespace Amber.Documents.Project.Mutations;

public record IncreaseCounter
{
    public required int Count { get; init; }
}