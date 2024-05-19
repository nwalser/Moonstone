using System.Globalization;

namespace Moonstone;

public record DocumentIdentity
{
    public required string Workspace { get; init; }
    public required Type Type { get; init; }
    public required int TypeId { get; init; }
    public required Guid Id { get; init; }

    public string GetPath()
    {
        return Path.Join(Workspace, TypeId.ToString(CultureInfo.InvariantCulture), Id.ToString());
    }
}