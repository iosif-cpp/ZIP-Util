using Kaspersky_Task2.Cli.Commands;

namespace Kaspersky_Task2.Cli;

public sealed class CommandDispatcher
{
    private readonly Dictionary<string, IClientCommand> _commands;

    public CommandDispatcher(IEnumerable<IClientCommand> commands)
    {
        _commands = new Dictionary<string, IClientCommand>(StringComparer.Ordinal);

        foreach (var cmd in commands)
        {
            _commands[cmd.Name] = cmd;
            foreach (var alias in cmd.Aliases)
                _commands[alias] = cmd;
        }
    }

    public bool TryGetCommand(string name, out IClientCommand? command)
    {
        command = default!;
        return _commands.TryGetValue(name, out command);
    }
}

