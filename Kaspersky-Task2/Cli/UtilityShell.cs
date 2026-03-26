namespace Kaspersky_Task2.Cli;

public sealed class UtilityShell
{
    private readonly ClientApp _clientApp;

    public UtilityShell(ClientApp clientApp)
    {
        _clientApp = clientApp;
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        while (true)
        {
            if (ct.IsCancellationRequested)
                return 1;

            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null)
                return 0;

            line = line.Trim();
            if (line.Length == 0)
                return 0;

            if (line.Equals("client", StringComparison.OrdinalIgnoreCase))
            {
                await _clientApp.RunInteractiveAsync(ct);
                return 0;
            }

            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                return 0;

            Console.WriteLine("Unknown command. Use: client");
        }
    }
}

