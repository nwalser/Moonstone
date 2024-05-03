﻿namespace Abstractions.Serializer;

public interface ITextSerializer<TType>
{
    public string Serialize(TType entry);
    public TType Deserialize(string text);
}