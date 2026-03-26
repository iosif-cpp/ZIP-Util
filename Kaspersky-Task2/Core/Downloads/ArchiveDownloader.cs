namespace Kaspersky_Task2.Core.Downloads;

public sealed class ArchiveDownloader
{
    public async Task<string> SaveZipAsync(
        Stream zipStream,
        string destinationFolder,
        string fileName,
        CancellationToken ct)
    {
        if (zipStream is null)
            throw new ArgumentNullException(nameof(zipStream));

        if (string.IsNullOrWhiteSpace(destinationFolder))
            throw new ArgumentException("Destination folder is required.", nameof(destinationFolder));

        Directory.CreateDirectory(destinationFolder);

        var safeFileName = string.IsNullOrWhiteSpace(fileName) ? "archive.zip" : fileName;
        var fullPath = Path.Combine(destinationFolder, safeFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await zipStream.CopyToAsync(fileStream, 81920, ct);
        await fileStream.FlushAsync(ct);
        return fullPath;
    }
}

