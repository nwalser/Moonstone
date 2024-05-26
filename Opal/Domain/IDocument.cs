﻿namespace Opal.Domain;

public interface IDocument
{
    public Guid Id { get; }
    public DateTime LastWrite { get; set; }
}