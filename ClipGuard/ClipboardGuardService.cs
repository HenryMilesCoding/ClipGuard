using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipGuard;

public sealed class ClipboardGuardService : NativeWindow, IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private readonly AppSettings _settings;
    private readonly PatternMatcher _matcher;

    private readonly System.Windows.Forms.Timer _timer;

    private bool _disposed;

    public event EventHandler<NotificationEventArgs>? NotificationRequested;

    public ClipboardGuardService(AppSettings settings)
    {
        _settings = settings;

        _matcher = new PatternMatcher(settings);

        CreateHandle(new CreateParams());

        _timer = new System.Windows.Forms.Timer();

        _timer.Tick += (_, _) =>
        {
            if (_settings.EnableAutoClear)
            {
                ClearClipboard();
            }
        };

        ApplySettings();
    }

    public void Start()
    {
        if (_disposed)
            return;

        AddClipboardFormatListener(Handle);

        ApplySettings();
    }

    public void ApplySettings()
    {
        if (_disposed)
            return;

        _matcher.Reload(_settings);

        _timer.Interval =
            Math.Max(1, _settings.AutoClearSeconds) * 1000;

        if (_settings.EnableMonitoring &&
            _settings.EnableAutoClear)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }
    }

    public void ClearClipboard()
    {
        if (_disposed)
            return;

        try
        {
            Clipboard.Clear();

            Notify(
                "ClipGuard",
                "Clipboard cleared.",
                ToolTipIcon.Info);
        }
        catch
        {
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (_disposed)
            return;

        if (m.Msg == WM_CLIPBOARDUPDATE)
        {
            HandleClipboard();
        }

        base.WndProc(ref m);
    }

    private void HandleClipboard()
    {
        try
        {
            if (!_settings.EnableMonitoring)
                return;

            if (!Clipboard.ContainsText())
                return;

            var text =
                Clipboard.GetText(TextDataFormat.UnicodeText);

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (_matcher.IsWhitelisted(text))
                return;

            if (_matcher.IsSensitive(text))
            {
                Clipboard.Clear();

                Notify(
                    "ClipGuard",
                    "Sensitive clipboard content removed.",
                    ToolTipIcon.Warning);
            }
        }
        catch
        {
        }
    }

    private void Notify(
        string title,
        string message,
        ToolTipIcon icon)
    {
        if (_disposed)
            return;

        if (!_settings.EnableNotifications)
            return;

        NotificationRequested?.Invoke(
            this,
            new NotificationEventArgs(
                title,
                message,
                icon));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            RemoveClipboardFormatListener(Handle);
        }
        catch
        {
        }

        _timer.Stop();
        _timer.Dispose();

        DestroyHandle();
    }
}

public sealed class NotificationEventArgs : EventArgs
{
    public string Title { get; }
    public string Message { get; }
    public ToolTipIcon Icon { get; }

    public NotificationEventArgs(
        string title,
        string message,
        ToolTipIcon icon)
    {
        Title = title;
        Message = message;
        Icon = icon;
    }
}