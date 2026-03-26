namespace Kaspersky_Task2.Cli.Commands;

public interface IClientCommand
{
    string Name { get; }
    string[] Aliases { get; }
    Task<int> ExecuteAsync(string[] args, CancellationToken ct);
}

