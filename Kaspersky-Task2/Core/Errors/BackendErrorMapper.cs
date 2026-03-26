using System.Net;
using System.Text.Json;

namespace Kaspersky_Task2.Core.Errors;

public sealed class BackendErrorMapper
{
    public BackendError Map(HttpStatusCode statusCode, string content)
    {
        var detail = ExtractDetail(content);

        var message = statusCode switch
        {
            HttpStatusCode.BadRequest => string.IsNullOrWhiteSpace(detail)
                ? "Input is invalid."
                : detail,
            HttpStatusCode.NotFound => "Process not found.",
            HttpStatusCode.Conflict => string.IsNullOrWhiteSpace(detail)
                ? "Archive is not ready yet."
                : detail,
            _ when (int)statusCode >= 500 => string.IsNullOrWhiteSpace(detail)
                ? "Backend error."
                : detail,
            _ => string.IsNullOrWhiteSpace(detail)
                ? $"Backend returned {(int)statusCode}."
                : detail
        };

        return new BackendError((int)statusCode, message);
    }

    private static string ExtractDetail(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        content = content.Trim();
        if (!content.StartsWith('{') && !content.StartsWith('['))
            return content.Trim('"');

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var property in errors.EnumerateObject())
                    {
                        var el = property.Value;
                        if (el.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in el.EnumerateArray())
                            {
                                var s = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                                if (!string.IsNullOrWhiteSpace(s))
                                    messages.Add(s);
                            }
                        }
                        else if (el.ValueKind == JsonValueKind.String)
                        {
                            var s = el.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                                messages.Add(s);
                        }
                    }

                    if (messages.Count > 0)
                        return string.Join("; ", messages);
                }

                if (root.TryGetProperty("detail", out var detailEl) && detailEl.ValueKind == JsonValueKind.String)
                {
                    var s = detailEl.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }

                if (root.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                {
                    var s = titleEl.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }
            }
        }
        catch
        {
        }

        return content;
    }
}

