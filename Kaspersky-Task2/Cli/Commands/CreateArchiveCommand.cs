using Kaspersky_Task2.Core.Api;

namespace Kaspersky_Task2.Cli.Commands;

public sealed class CreateArchiveCommand : IClientCommand
{
    private readonly IBackendApiClient _api;

    public CreateArchiveCommand(IBackendApiClient api)
    {
        _api = api;
    }

    public string Name => "create-archive";
    public string[] Aliases => Array.Empty<string>();

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: create-archive <file1> <file2> ...");
            return 1;
        }

        var files = args.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
        if (files.Length == 0)
        {
            Console.WriteLine("Usage: create-archive <file1> <file2> ...");
            return 1;
        }

        var id = await _api.CreateArchiveAsync(files, ct);
        Console.WriteLine($"Create archive task is started, id: {id}");
        return 0;
    }
}

