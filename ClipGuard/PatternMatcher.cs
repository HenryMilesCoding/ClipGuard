using System.Text.RegularExpressions;

namespace ClipGuard;

public sealed class PatternMatcher
{
    private readonly object _sync = new();

    private List<Regex> _sensitiveRegexes = new();
    private List<Regex> _whitelistRegexes = new();

    public PatternMatcher(AppSettings settings)
    {
        Reload(settings);
    }

    public void Reload(AppSettings settings)
    {
        lock (_sync)
        {
            _sensitiveRegexes = BuildRegexList(settings.SensitivePatterns);
            _whitelistRegexes = BuildRegexList(settings.WhitelistPatterns);
        }
    }

    public bool IsWhitelisted(string text)
    {
        lock (_sync)
        {
            return _whitelistRegexes.Any(r => r.IsMatch(text));
        }
    }

    public bool IsSensitive(string text)
    {
        lock (_sync)
        {
            return _sensitiveRegexes.Any(r => r.IsMatch(text));
        }
    }

    private static List<Regex> BuildRegexList(IEnumerable<string> patterns)
    {
        var list = new List<Regex>();

        foreach (var rawPattern in patterns)
        {
            var pattern = rawPattern?.Trim();
            if (string.IsNullOrWhiteSpace(pattern))
                continue;

            try
            {
                list.Add(new Regex(pattern,
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant |
                    RegexOptions.Compiled));
            }
            catch (ArgumentException)
            {
                var literal = Regex.Escape(pattern);

                try
                {
                    list.Add(new Regex(literal,
                        RegexOptions.IgnoreCase |
                        RegexOptions.CultureInvariant |
                        RegexOptions.Compiled));
                }
                catch
                {
                }
            }
        }

        return list;
    }
}