namespace Kaspersky_Task2.Core.Api.Dtos;

public sealed record ArchiveStatusResponseDto(
    Guid ProcessId,
    string Status,
    string? Error);

