using Kaspersky_Task2.Core.Api;

namespace Kaspersky_Task2.Cli.Commands;

public sealed class DownloadCommand : IClientCommand
{
    private readonly IBackendApiClient _api;

    public DownloadCommand(IBackendApiClient api)
    {
        _api = api;
    }

    public string Name => "download";
    public string[] Aliases => Array.Empty<string>();

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct)
    {
        if (args.Length != 2 || !Guid.TryParse(args[0], out var processId))
        {
            Console.WriteLine("Usage: download <processId> <path>");
            return 1;
        }

        var path = args[1];
        var savedPath = await _api.DownloadArchiveAsync(processId, path, ct);
        Console.WriteLine($"Archive has been downloaded: {savedPath}");
        return 0;
    }
}

