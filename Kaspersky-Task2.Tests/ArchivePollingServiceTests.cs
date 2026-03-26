using System.Net;
using Kaspersky_Task2.Core.Api;
using Kaspersky_Task2.Core.Api.Dtos;
using Kaspersky_Task2.Core.Polling;
using Xunit;

namespace Kaspersky_Task2.Tests;

public sealed class ArchivePollingServiceTests
{
    private sealed class FakeBackendApiClient : IBackendApiClient
    {
        private readonly Queue<ArchiveStatusResponseDto> _statuses;
        private readonly Guid _processId;
        public int DownloadCount { get; private set; }
        public string DownloadPath { get; private set; } = string.Empty;

        public FakeBackendApiClient(Guid processId, IEnumerable<ArchiveStatusResponseDto> statuses)
        {
            _processId = processId;
            _statuses = new Queue<ArchiveStatusResponseDto>(statuses);
        }

        public Task<IReadOnlyList<string>> ListFilesAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        public Task<Guid> CreateArchiveAsync(IReadOnlyList<string> files, CancellationToken ct) =>
            Task.FromResult(_processId);

        public Task<ArchiveStatusResponseDto> GetStatusAsync(Guid processId, CancellationToken ct)
        {
            if (_statuses.Count == 0)
                return Task.FromResult(new ArchiveStatusResponseDto(processId, "done", null));

            return Task.FromResult(_statuses.Dequeue());
        }

        public Task<string> DownloadArchiveAsync(Guid processId, string destinationFolder, CancellationToken ct)
        {
            DownloadCount++;
            DownloadPath = Path.Combine(destinationFolder, $"archive-{processId}.zip");
            return Task.FromResult(DownloadPath);
        }
    }

    [Fact]
    public async Task CreateAndDownloadAsync_ProcessingThenDone_DownloadCalledOnce()
    {
        var processId = Guid.NewGuid();
        var statuses = new[]
        {
            new ArchiveStatusResponseDto(processId, "processing", null),
            new ArchiveStatusResponseDto(processId, "done", null)
        };

        var fake = new FakeBackendApiClient(processId, statuses);
        var service = new ArchivePollingService(fake);

        var outDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var savedPath = await service.CreateAndDownloadAsync(
                new[] { "a.txt" },
                outDir,
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromSeconds(1),
                CancellationToken.None);

            Assert.Equal(1, fake.DownloadCount);
            Assert.Equal(Path.Combine(outDir, $"archive-{processId}.zip"), savedPath);
        }
        finally
        {
            if (Directory.Exists(outDir))
                Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public async Task CreateAndDownloadAsync_Failed_ThrowsArchiveFailedException()
    {
        var processId = Guid.NewGuid();
        var statuses = new[]
        {
            new ArchiveStatusResponseDto(processId, "failed", "Backend says no.")
        };

        var fake = new FakeBackendApiClient(processId, statuses);
        var service = new ArchivePollingService(fake);

        var outDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            await Assert.ThrowsAsync<ArchiveFailedException>(() =>
                service.CreateAndDownloadAsync(
                    new[] { "a.txt" },
                    outDir,
                    TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromSeconds(1),
                    CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(outDir))
                Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public async Task CreateAndDownloadAsync_Timeout_ThrowsArchiveTimeoutException()
    {
        var processId = Guid.NewGuid();
        var statuses = Enumerable.Repeat(
            new ArchiveStatusResponseDto(processId, "processing", null),
            1000).ToArray();

        var fake = new FakeBackendApiClient(processId, statuses);
        var service = new ArchivePollingService(fake);

        var outDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            await Assert.ThrowsAsync<ArchiveTimeoutException>(() =>
                service.CreateAndDownloadAsync(
                    new[] { "a.txt" },
                    outDir,
                    TimeSpan.FromMilliseconds(10),
                    TimeSpan.FromMilliseconds(30),
                    CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(outDir))
                Directory.Delete(outDir, recursive: true);
        }
    }
}

