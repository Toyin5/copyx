namespace CopyX.Utilities;

public static class PathHelper
{
    public static string ResolveSingleFileDestination(string sourceFilePath, string destinationPath)
    {
        if (Directory.Exists(destinationPath))
        {
            return Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));
        }

        return destinationPath;
    }
}
