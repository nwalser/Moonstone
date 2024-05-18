﻿namespace Amber.Domain.Documents.Todo;

public class TodoAggregate(string name)
{
    public string Name { get; set; } = name;
    public TimeSpan EstimatedEffort { get; set; } = TimeSpan.Zero;
    public bool Completed { get; set; } = false;
}