using Moonstone.Workspace.Data;

namespace Moonstone.Workspace.MutationStream;

public interface IMutationStream
{
    public void Append(MutationEnvelope entry, CancellationToken ct = default);
}