using Kaspersky_Task2.Core.Api;
using Kaspersky_Task2.Core.Api.Dtos;

namespace Kaspersky_Task2.Core.Polling;

public sealed class ArchivePollingService
{
    private readonly IBackendApiClient _api;

    public ArchivePollingService(IBackendApiClient api)
    {
        _api = api;
    }

    public async Task<string> CreateAndDownloadAsync(
        IReadOnlyList<string> files,
        string destinationFolder,
        TimeSpan pollInterval,
        TimeSpan timeout,
        CancellationToken ct)
    {
        var processId = await _api.CreateArchiveAsync(files, ct);
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var status = await _api.GetStatusAsync(processId, ct);
            var state = status.Status?.Trim().ToLowerInvariant() ?? string.Empty;

            if (state == "done")
                break;

            if (state == "failed")
                throw new ArchiveFailedException(status.Error ?? "Archive job failed.");

            if (DateTimeOffset.UtcNow >= deadline)
                throw new ArchiveTimeoutException("Archive creation timed out.");

            var remaining = deadline - DateTimeOffset.UtcNow;
            var delay = remaining < pollInterval ? remaining : pollInterval;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
        }

        return await _api.DownloadArchiveAsync(processId, destinationFolder, ct);
    }

    public async Task<ArchiveStatusResponseDto> WaitForStatusAsync(
        Guid processId,
        TimeSpan pollInterval,
        TimeSpan timeout,
        CancellationToken ct)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var status = await _api.GetStatusAsync(processId, ct);
            var state = status.Status?.Trim().ToLowerInvariant() ?? string.Empty;

            if (state == "done" || state == "failed")
                return status;

            if (DateTimeOffset.UtcNow >= deadline)
                throw new ArchiveTimeoutException("Archive creation timed out.");

            var remaining = deadline - DateTimeOffset.UtcNow;
            var delay = remaining < pollInterval ? remaining : pollInterval;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
        }
    }
}

