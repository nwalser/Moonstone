﻿using System.Diagnostics;

namespace Moonstone.Framework.Stream;

public abstract class Mutation
{
    public Guid Id { get; init; }
    public DateTime Occurence { get; init; }

    public Mutation()
    {
        Id = Guid.NewGuid();
        Occurence = new DateTime(Stopwatch.GetTimestamp());
    }
}