using System.Text.Json;

namespace Kaspersky_Task2.Options;

public sealed class ClientOptions
{
    public string BaseUrl { get; init; } = "http://localhost:5278";
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(2);
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(10);
    public JsonSerializerOptions JsonOptions { get; init; } = CreateJsonOptions();

    public static ClientOptions FromEnvironmentAndArgs(string[] args)
    {
        var baseUrl =
            Environment.GetEnvironmentVariable("KASPERSKY_BACKEND_URL") ??
            Environment.GetEnvironmentVariable("BACKEND_URL") ??
            "http://localhost:5278";

        var pollInterval = TimeSpan.FromSeconds(2);
        var timeout = TimeSpan.FromMinutes(10);

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--base-url" || args[i] == "--backend-url")
            {
                if (i + 1 < args.Length)
                    baseUrl = args[i + 1];
            }
        }

        return new ClientOptions
        {
            BaseUrl = NormalizeBaseUrl(baseUrl),
            PollInterval = pollInterval,
            Timeout = timeout,
            JsonOptions = CreateJsonOptions()
        };
    }

    private static string NormalizeBaseUrl(string url)
    {
        url = url.Trim();
        if (url.EndsWith('/'))
            url = url.TrimEnd('/');
        return url;
    }

    public static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
}

