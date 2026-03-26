namespace Kaspersky_Task2.Core.Errors;

public sealed class BackendError
{
    public BackendError(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public int StatusCode { get; }
    public string Message { get; }
}

