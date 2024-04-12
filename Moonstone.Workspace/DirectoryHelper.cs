namespace Moonstone.Workspace;

public static class DirectoryHelper
{
    public static bool IsSubdirectory(string parentPath, string potentialSubPath)
    {
        var relativePath = Path.GetRelativePath(parentPath, potentialSubPath);
        return !relativePath.StartsWith("..\\");
    }
}