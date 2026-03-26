namespace Kaspersky_Task2.Core.Errors;

public sealed class BackendApiException : Exception
{
    public BackendApiException(BackendError error)
        : base(error.Message)
    {
        Error = error;
    }

    public BackendError Error { get; }
}

