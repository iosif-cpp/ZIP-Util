using Kaspersky_Task2.Core.Errors;
using Kaspersky_Task2.Cli.Commands;

namespace Kaspersky_Task2.Cli;

public sealed class ClientApp
{
    private readonly InteractiveShell _interactiveShell;
    private readonly CommandDispatcher _dispatcher;

    public ClientApp(InteractiveShell interactiveShell, CommandDispatcher dispatcher)
    {
        _interactiveShell = interactiveShell;
        _dispatcher = dispatcher;
    }

    public Task RunInteractiveAsync(CancellationToken ct) => _interactiveShell.RunAsync(ct);

    public async Task<int> RunPosixAsync(string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
            return 0;

        var cmdIndex = -1;
        IClientCommand? command = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (_dispatcher.TryGetCommand(args[i], out var found) && found is not null)
            {
                cmdIndex = i;
                command = found;
                break;
            }
        }

        if (cmdIndex < 0 || command is null)
        {
            Console.WriteLine("Unknown command.");
            return 1;
        }

        var cmdArgs = args.Skip(cmdIndex + 1).ToArray();

        try
        {
            return await command.ExecuteAsync(cmdArgs, ct);
        }
        catch (BackendApiException ex)
        {
            Console.WriteLine($"{ex.Error.StatusCode}: {ex.Error.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}

