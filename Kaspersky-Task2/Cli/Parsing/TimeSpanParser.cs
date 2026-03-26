namespace Kaspersky_Task2.Cli.Parsing;

public static class TimeSpanParser
{
    public static bool TryParse(string? value, out TimeSpan result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        if (TimeSpan.TryParse(value, out result))
            return true;

        var suffix = value.Length >= 2 ? value[^2..] : string.Empty;
        if (suffix.Equals("ms", StringComparison.OrdinalIgnoreCase) && value.Length > 2)
        {
            var num = value[..^2].ToString();
            if (int.TryParse(num, out var ms))
            {
                result = TimeSpan.FromMilliseconds(ms);
                return true;
            }
        }

        if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase) && value.Length > 1)
        {
            var num = value[..^1].ToString();
            if (int.TryParse(num, out var s))
            {
                result = TimeSpan.FromSeconds(s);
                return true;
            }
        }

        if (value.EndsWith("m", StringComparison.OrdinalIgnoreCase) && value.Length > 1)
        {
            var num = value[..^1].ToString();
            if (int.TryParse(num, out var m))
            {
                result = TimeSpan.FromMinutes(m);
                return true;
            }
        }

        if (value.EndsWith("h", StringComparison.OrdinalIgnoreCase) && value.Length > 1)
        {
            var num = value[..^1].ToString();
            if (int.TryParse(num, out var h))
            {
                result = TimeSpan.FromHours(h);
                return true;
            }
        }

        return false;
    }
}

