namespace Kaspersky_Task2.Core.Polling;

public sealed class ArchiveTimeoutException : TimeoutException
{
    public ArchiveTimeoutException(string message)
        : base(message)
    {
    }
}

