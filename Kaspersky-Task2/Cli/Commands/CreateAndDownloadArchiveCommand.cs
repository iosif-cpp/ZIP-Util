using Kaspersky_Task2.Cli.Parsing;
using Kaspersky_Task2.Core.Api;
using Kaspersky_Task2.Core.Polling;
using Kaspersky_Task2.Options;

namespace Kaspersky_Task2.Cli.Commands;

public sealed class CreateAndDownloadArchiveCommand : IClientCommand
{
    private readonly IBackendApiClient _api;
    private readonly ClientOptions _defaults;
    private readonly ArchivePollingService _polling;

    public CreateAndDownloadArchiveCommand(IBackendApiClient api, ClientOptions defaults)
    {
        _api = api;
        _defaults = defaults;
        _polling = new ArchivePollingService(api);
    }

    public string Name => "create-and-download-archive";
    public string[] Aliases => new[] { "create-archive-and-download" };

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct)
    {
        var files = new List<string>();
        var outDir = Directory.GetCurrentDirectory();
        var pollInterval = _defaults.PollInterval;
        var timeout = _defaults.Timeout;

        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (token == "--out" && i + 1 < args.Length)
            {
                outDir = args[i + 1];
                i++;
                continue;
            }

            if (token == "--poll-interval" && i + 1 < args.Length)
            {
                var v = args[i + 1];
                if (TimeSpanParser.TryParse(v, out var ts))
                    pollInterval = ts;
                i++;
                continue;
            }

            if (token == "--timeout" && i + 1 < args.Length)
            {
                var v = args[i + 1];
                if (TimeSpanParser.TryParse(v, out var ts))
                    timeout = ts;
                i++;
                continue;
            }

            files.Add(token);
        }

        files.RemoveAll(string.IsNullOrWhiteSpace);
        if (files.Count == 0)
        {
            Console.WriteLine("Usage: create-and-download-archive <file1> <file2> ... --out <path>");
            return 1;
        }

        var processId = await _api.CreateArchiveAsync(files, ct);
        Console.WriteLine($"Create archive task is started, id: {processId}");

        var status = await _polling.WaitForStatusAsync(processId, pollInterval, timeout, ct);
        var state = status.Status?.Trim().ToLowerInvariant() ?? string.Empty;

        if (state == "failed")
            throw new ArchiveFailedException(status.Error ?? "Archive job failed.");

        Console.WriteLine("Archive has been created.");

        var savedPath = await _api.DownloadArchiveAsync(processId, outDir, ct);
        Console.WriteLine($"Archive has been downloaded: {savedPath}");
        return 0;
    }
}

