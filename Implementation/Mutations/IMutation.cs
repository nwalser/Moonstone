namespace Implementation.Mutations;

[MessagePack.Union(0, typeof(CreateTask))]
[MessagePack.Union(1, typeof(DeleteTask))]
public interface IMutation { }