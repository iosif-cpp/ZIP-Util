using Kaspersky_Task2.Core.Api;

namespace Kaspersky_Task2.Cli.Commands;

public sealed class StatusCommand : IClientCommand
{
    private readonly IBackendApiClient _api;

    public StatusCommand(IBackendApiClient api)
    {
        _api = api;
    }

    public string Name => "status";
    public string[] Aliases => Array.Empty<string>();

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct)
    {
        if (args.Length != 1 || !Guid.TryParse(args[0], out var processId))
        {
            Console.WriteLine("Usage: status <processId>");
            return 1;
        }

        var status = await _api.GetStatusAsync(processId, ct);
        var state = status.Status?.Trim().ToLowerInvariant() ?? string.Empty;

        if (state == "pending" || state == "processing")
        {
            Console.WriteLine("Process in progress, please wait…");
            return 0;
        }

        if (state == "done")
        {
            Console.WriteLine("Archive has been created.");
            return 0;
        }

        if (state == "failed")
        {
            Console.WriteLine(status.Error ?? "Archive job failed.");
            return 0;
        }

        Console.WriteLine(status.Status ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(status.Error))
            Console.WriteLine(status.Error);
        return 0;
    }
}

