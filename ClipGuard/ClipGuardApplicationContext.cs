using System.Drawing;
using System.Windows.Forms;

namespace ClipGuard;

public sealed class ClipGuardApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;

    private readonly AppSettings _settings;
    private readonly ClipboardGuardService _clipboardService;

    private readonly ToolStripMenuItem _monitoringItem;
    private readonly ToolStripMenuItem _autoClearItem;
    private readonly ToolStripMenuItem _notificationsItem;

    public ClipGuardApplicationContext()
    {
        _settings = SettingsStore.Load();

        _clipboardService = new ClipboardGuardService(_settings);

        _clipboardService.NotificationRequested += OnNotificationRequested;

        _menu = new ContextMenuStrip();

        _monitoringItem = new ToolStripMenuItem("Monitoring Enabled")
        {
            Checked = _settings.EnableMonitoring,
            CheckOnClick = true
        };

        _monitoringItem.CheckedChanged += (_, _) =>
        {
            _settings.EnableMonitoring = _monitoringItem.Checked;

            SettingsStore.Save(_settings);
            _clipboardService.ApplySettings();
        };

        _autoClearItem = new ToolStripMenuItem("Auto Clear")
        {
            Checked = _settings.EnableAutoClear,
            CheckOnClick = true
        };

        _autoClearItem.CheckedChanged += (_, _) =>
        {
            _settings.EnableAutoClear = _autoClearItem.Checked;

            SettingsStore.Save(_settings);
            _clipboardService.ApplySettings();
        };

        _notificationsItem = new ToolStripMenuItem("Notifications")
        {
            Checked = _settings.EnableNotifications,
            CheckOnClick = true
        };

        _notificationsItem.CheckedChanged += (_, _) =>
        {
            _settings.EnableNotifications = _notificationsItem.Checked;

            SettingsStore.Save(_settings);
        };

        var settingsItem = new ToolStripMenuItem("Regex Settings");

        settingsItem.Click += (_, _) =>
        {
            using var form = new SettingsForm(_settings);

            if (form.ShowDialog() == DialogResult.OK)
            {
                SettingsStore.Save(_settings);

                _clipboardService.ApplySettings();

                ShowNotification(
                    "ClipGuard",
                    "Settings saved.",
                    ToolTipIcon.Info);
            }
        };

        var clearItem = new ToolStripMenuItem("Clear Clipboard");

        clearItem.Click += (_, _) =>
        {
            _clipboardService.ClearClipboard();
        };

        var exitItem = new ToolStripMenuItem("Exit");

        exitItem.Click += (_, _) =>
        {
            ExitThread();
        };

        _menu.Items.Add(_monitoringItem);
        _menu.Items.Add(_autoClearItem);
        _menu.Items.Add(_notificationsItem);

        _menu.Items.Add(new ToolStripSeparator());

        _menu.Items.Add(settingsItem);

        _menu.Items.Add(new ToolStripSeparator());

        _menu.Items.Add(clearItem);

        _menu.Items.Add(new ToolStripSeparator());

        _menu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Text = "ClipGuard",
            Visible = true,
            ContextMenuStrip = _menu
        };

        _clipboardService.Start();
    }

    private void OnNotificationRequested(
        object? sender,
        NotificationEventArgs e)
    {
        ShowNotification(
            e.Title,
            e.Message,
            e.Icon);
    }

    private void ShowNotification(
        string title,
        string message,
        ToolTipIcon icon)
    {
        if (!_settings.EnableNotifications)
            return;

        _trayIcon.BalloonTipTitle = title;
        _trayIcon.BalloonTipText = message;
        _trayIcon.BalloonTipIcon = icon;

        _trayIcon.ShowBalloonTip(3000);
    }

    protected override void ExitThreadCore()
    {
        _clipboardService.NotificationRequested -= OnNotificationRequested;

        _clipboardService.Dispose();

        _trayIcon.Visible = false;
        _trayIcon.Dispose();

        _menu.Dispose();

        base.ExitThreadCore();
    }
}