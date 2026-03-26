using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using Kaspersky_Task2.Core.Api.Dtos;
using Kaspersky_Task2.Core.Downloads;
using Kaspersky_Task2.Core.Errors;
using Kaspersky_Task2.Options;

namespace Kaspersky_Task2.Core.Api;

public sealed class BackendApiClient : IBackendApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly BackendErrorMapper _errorMapper;
    private readonly ArchiveDownloader _downloader;

    public BackendApiClient(
        HttpClient httpClient,
        ClientOptions options,
        BackendErrorMapper errorMapper,
        ArchiveDownloader downloader)
    {
        _httpClient = httpClient;
        _jsonOptions = options.JsonOptions;
        _errorMapper = errorMapper;
        _downloader = downloader;
    }

    public async Task<IReadOnlyList<string>> ListFilesAsync(CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync("/api/files", ct);
        if (!response.IsSuccessStatusCode)
            throw await CreateBackendException(response, ct);

        var result = await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions, ct);
        return (IReadOnlyList<string>)(result ?? new List<string>());
    }

    public async Task<Guid> CreateArchiveAsync(IReadOnlyList<string> files, CancellationToken ct)
    {
        var request = new InitArchiveRequestDto(files);
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("/api/archives/init", content, ct);
        if (!response.IsSuccessStatusCode)
            throw await CreateBackendException(response, ct);

        var result = await response.Content.ReadFromJsonAsync<InitArchiveResponseDto>(_jsonOptions, ct);
        if (result is null)
            throw new BackendApiException(new BackendError((int)HttpStatusCode.InternalServerError, "Invalid response from backend."));

        return result.ProcessId;
    }

    public async Task<ArchiveStatusResponseDto> GetStatusAsync(Guid processId, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync($"/api/archives/{processId}/status", ct);
        if (!response.IsSuccessStatusCode)
            throw await CreateBackendException(response, ct);

        var result = await response.Content.ReadFromJsonAsync<ArchiveStatusResponseDto>(_jsonOptions, ct);
        if (result is null)
            throw new BackendApiException(new BackendError((int)HttpStatusCode.InternalServerError, "Invalid response from backend."));

        return result;
    }

    public async Task<string> DownloadArchiveAsync(Guid processId, string destinationFolder, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(
            $"/api/archives/{processId}/download",
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateBackendException(response, ct);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await _downloader.SaveZipAsync(stream, destinationFolder, $"archive-{processId}.zip", ct);
    }

    private async Task<BackendApiException> CreateBackendException(HttpResponseMessage response, CancellationToken ct)
    {
        var body = string.Empty;
        try
        {
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
        }

        var error = _errorMapper.Map(response.StatusCode, body);
        return new BackendApiException(error);
    }
}

