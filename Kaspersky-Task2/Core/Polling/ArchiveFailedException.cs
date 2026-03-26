namespace Kaspersky_Task2.Core.Polling;

public sealed class ArchiveFailedException : Exception
{
    public ArchiveFailedException(string message)
        : base(message)
    {
    }
}

