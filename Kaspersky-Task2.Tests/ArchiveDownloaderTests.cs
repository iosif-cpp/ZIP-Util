using Kaspersky_Task2.Core.Downloads;
using Xunit;

namespace Kaspersky_Task2.Tests;

public sealed class ArchiveDownloaderTests
{
    [Fact]
    public async Task SaveZipAsync_WritesFileBytes()
    {
        var downloader = new ArchiveDownloader();
        var outDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outDir);

        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        await using var ms = new MemoryStream(bytes);

        var savedPath = await downloader.SaveZipAsync(ms, outDir, "test.zip", CancellationToken.None);

        Assert.True(File.Exists(savedPath));
        var savedBytes = await File.ReadAllBytesAsync(savedPath);
        Assert.Equal(bytes, savedBytes);

        Directory.Delete(outDir, recursive: true);
    }
}

