using CopyX.Models;
using CopyX.Utilities;

namespace CopyX.Services;

public sealed class FileScanner
{
    public IReadOnlyList<CopyItem> Scan(CopyOptions options)
    {
        var source = Path.GetFullPath(options.SourcePath);
        var destination = Path.GetFullPath(options.DestinationPath);

        if (File.Exists(source))
        {
            return
            [
                new CopyItem
                {
                    SourcePath = source,
                    DestinationPath = PathHelper.ResolveSingleFileDestination(source, destination),
                    SizeInBytes = new FileInfo(source).Length
                }
            ];
        }

        if (Directory.Exists(source))
        {
            return Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
                .Select(file =>
                {
                    var relativePath = Path.GetRelativePath(source, file);
                    return new CopyItem
                    {
                        SourcePath = file,
                        DestinationPath = Path.Combine(destination, relativePath),
                        SizeInBytes = new FileInfo(file).Length
                    };
                })
                .OrderBy(item => item.SourcePath, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        throw new FileNotFoundException("Source file or directory does not exist.", options.SourcePath);
    }
}
