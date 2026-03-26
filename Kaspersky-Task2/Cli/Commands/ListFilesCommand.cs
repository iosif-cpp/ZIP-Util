using Kaspersky_Task2.Core.Api;

namespace Kaspersky_Task2.Cli.Commands;

public sealed class ListFilesCommand : IClientCommand
{
    private readonly IBackendApiClient _api;

    public ListFilesCommand(IBackendApiClient api)
    {
        _api = api;
    }

    public string Name => "list";
    public string[] Aliases => Array.Empty<string>();

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct)
    {
        if (args.Length != 0)
        {
            Console.WriteLine("Usage: list");
            return 1;
        }

        var files = await _api.ListFilesAsync(ct);
        Console.WriteLine(string.Join(' ', files));
        return 0;
    }
}

