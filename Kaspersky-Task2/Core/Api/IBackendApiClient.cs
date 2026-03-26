using Kaspersky_Task2.Core.Api.Dtos;

namespace Kaspersky_Task2.Core.Api;

public interface IBackendApiClient
{
    Task<IReadOnlyList<string>> ListFilesAsync(CancellationToken ct);
    Task<Guid> CreateArchiveAsync(IReadOnlyList<string> files, CancellationToken ct);
    Task<ArchiveStatusResponseDto> GetStatusAsync(Guid processId, CancellationToken ct);
    Task<string> DownloadArchiveAsync(Guid processId, string destinationFolder, CancellationToken ct);
}

