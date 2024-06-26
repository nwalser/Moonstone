﻿using ProtoBuf;

namespace Moonstone.Database.Deltas;


[ProtoContract]
[ProtoInclude(1, typeof(Update))]
[ProtoInclude(2, typeof(Delete))]
public interface IDelta
{
    DateTime Timestamp { get; }
    int TypeId { get; }
    Guid RowId { get; }
}