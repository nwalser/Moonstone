﻿namespace Moonstone;

public abstract class Handler<TDocument> : IHandler
{
    public abstract int DocumentTypeId { get; }
    public abstract Type DocumentType { get; }
    public abstract Dictionary<int, Type> MutationTypes { get; }

    object IHandler.CreateNew()
    {
        return CreateNew() ?? throw new InvalidOperationException();
    }

    void IHandler.ApplyMutation(object aggregate, object mutation)
    {
        ApplyMutation((TDocument)aggregate, mutation);
    }

    public abstract TDocument CreateNew();
    public abstract void ApplyMutation(TDocument project, object mutation);
}