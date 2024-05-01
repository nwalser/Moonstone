namespace Framework;

public interface IMutationStream
{
    public void Append(MutationLogEntry entry, CancellationToken ct = default);
}