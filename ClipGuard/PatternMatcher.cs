using System.Text.RegularExpressions;

namespace ClipGuard;

public sealed class PatternMatcher
{
    private List<Regex> _sensitive = new();
    private List<Regex> _whitelist = new();

    public PatternMatcher(AppSettings settings)
    {
        Reload(settings);
    }

    public void Reload(AppSettings settings)
    {
        _sensitive = Build(settings.SensitivePatterns);
        _whitelist = Build(settings.WhitelistPatterns);
    }

    public bool IsSensitive(string text)
    {
        return _sensitive.Any(x => x.IsMatch(text));
    }

    public bool IsWhitelisted(string text)
    {
        return _whitelist.Any(x => x.IsMatch(text));
    }

    private static List<Regex> Build(IEnumerable<string> patterns)
    {
        var list = new List<Regex>();

        foreach (var pattern in patterns)
        {
            try
            {
                list.Add(new Regex(
                    pattern,
                    RegexOptions.IgnoreCase |
                    RegexOptions.Compiled |
                    RegexOptions.CultureInvariant));
            }
            catch
            {
            }
        }

        return list;
    }
}