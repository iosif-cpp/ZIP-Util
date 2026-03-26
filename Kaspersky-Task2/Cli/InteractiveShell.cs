using Kaspersky_Task2.Core.Errors;

namespace Kaspersky_Task2.Cli;

public sealed class InteractiveShell
{
    private readonly CommandDispatcher _dispatcher;

    public InteractiveShell(CommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine("Client was started.");
        Console.WriteLine("Press <Enter> to exit...");

        while (true)
        {
            if (ct.IsCancellationRequested)
                return;

            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null)
                return;

            if (string.IsNullOrWhiteSpace(line))
                return;

            var tokens = SplitTokens(line);
            if (tokens.Length == 0)
                continue;

            var cmdName = tokens[0];
            var cmdArgs = tokens.Skip(1).ToArray();

            if (!_dispatcher.TryGetCommand(cmdName, out var command) || command is null)
            {
                Console.WriteLine("Unknown command.");
                continue;
            }

            try
            {
                await command.ExecuteAsync(cmdArgs, ct);
            }
            catch (BackendApiException ex)
            {
                Console.WriteLine($"{ex.Error.StatusCode}: {ex.Error.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static string[] SplitTokens(string line)
    {
        return line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

