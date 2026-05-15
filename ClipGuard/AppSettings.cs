namespace ClipGuard;

public sealed class AppSettings
{
    public bool EnableMonitoring { get; set; } = true;
    public bool EnableAutoClear { get; set; } = true;
    public int AutoClearSeconds { get; set; } = 10;
    public bool EnableNotifications { get; set; } = true;

    public List<string> SensitivePatterns { get; set; } = new()
    {
        @"password",
        @"passwd",
        @"token",
        @"api[-_ ]?key",
        @"bearer\s+[A-Za-z0-9\-\._~\+/]+=*"
    };

    public List<string> WhitelistPatterns { get; set; } = new()
    {
        @"localhost",
        @"127\.0\.0\.1",
        @"example",
        @"test"
    };
}