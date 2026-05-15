using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipGuard;

public sealed class ClipboardGuardService : IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private readonly AppSettings _settings;
    private readonly PatternMatcher _patternMatcher;
    private readonly System.Windows.Forms.Timer _autoClearTimer;

    private bool _isListening;
    private bool _suppressClipboardEvent;
    private IntPtr _windowHandle = IntPtr.Zero;

    public event EventHandler<NotificationEventArgs>? NotificationRequested;

    public event EventHandler<string>? StatusRequested;

    public ClipboardGuardService(AppSettings settings)
    {
        _settings = settings;
        _patternMatcher = new PatternMatcher(settings);

        _autoClearTimer = new System.Windows.Forms.Timer();
        _autoClearTimer.Tick += (_, _) =>
        {
            if (_settings.EnableAutoClear)
                ClearClipboardSafe("Die Zwischenablage wurde automatisch geleert.");
        };

        ApplySettings();
    }

    public void Attach(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    public void Start()
    {
        if (_windowHandle == IntPtr.Zero)
            return;

        if (!_isListening)
        {
            AddClipboardFormatListener(_windowHandle);
            _isListening = true;
        }

        ApplySettings();
    }

    public void Stop()
    {
        if (_isListening && _windowHandle != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(_windowHandle);
            _isListening = false;
        }

        _autoClearTimer.Stop();
    }

    public void ApplySettings()
    {
        _patternMatcher.Reload(_settings);

        _autoClearTimer.Interval = Math.Max(1, _settings.AutoClearSeconds) * 1000;

        if (_settings.EnableMonitoring && _settings.EnableAutoClear)
            _autoClearTimer.Start();
        else
            _autoClearTimer.Stop();
    }

    public void ProcessWindowMessage(ref Message m)
    {
        if (m.Msg == WM_CLIPBOARDUPDATE)
            HandleClipboardUpdate();
    }

    public void ClearClipboardManually()
    {
        ClearClipboardSafe("Die Zwischenablage wurde geleert.");
    }

    private void HandleClipboardUpdate()
    {
        if (_suppressClipboardEvent || !_settings.EnableMonitoring)
            return;

        try
        {
            if (!Clipboard.ContainsText())
                return;

            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (_patternMatcher.IsWhitelisted(text))
            {
                StatusRequested?.Invoke(this, "Whitelisted-Inhalt erkannt, kein Eingriff.");
                return;
            }

            if (_patternMatcher.IsSensitive(text))
            {
                _suppressClipboardEvent = true;
                Clipboard.Clear();

                StatusRequested?.Invoke(this, "Sensibler Inhalt erkannt und entfernt.");
                Notify("ClipGuard", "Sensibler Inhalt wurde aus der Zwischenablage entfernt.", ToolTipIcon.Warning);
            }
        }
        catch
        {
            // Clipboard kann kurzfristig gesperrt sein.
        }
        finally
        {
            _suppressClipboardEvent = false;
        }
    }

    private void ClearClipboardSafe(string? statusMessage = null)
    {
        try
        {
            _suppressClipboardEvent = true;
            Clipboard.Clear();

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                StatusRequested?.Invoke(this, statusMessage);
                Notify("ClipGuard", statusMessage, ToolTipIcon.Info);
            }
        }
        catch
        {
            // bewusst still
        }
        finally
        {
            _suppressClipboardEvent = false;
        }
    }

    private void Notify(string title, string message, ToolTipIcon icon)
    {
        if (!_settings.EnableNotifications)
            return;

        NotificationRequested?.Invoke(this, new NotificationEventArgs(title, message, icon));
    }

    public void Dispose()
    {
        Stop();
        _autoClearTimer.Dispose();
    }
}

public sealed class NotificationEventArgs : EventArgs
{
    public NotificationEventArgs(string title, string message, ToolTipIcon icon)
    {
        Title = title;
        Message = message;
        Icon = icon;
    }

    public string Title { get; }
    public string Message { get; }
    public ToolTipIcon Icon { get; }
}