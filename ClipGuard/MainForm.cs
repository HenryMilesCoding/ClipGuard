using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

using System.Windows.Forms;

namespace ClipGuard;

public partial class MainForm : Form
{
    private readonly AppSettings _settings;
    private readonly ClipboardGuardService _clipboardGuardService;
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly ToolStripMenuItem _monitoringItem;
    private readonly ToolStripMenuItem _autoClearItem;
    private readonly ToolStripMenuItem _notificationsItem;

    private bool _ready;

    public MainForm()
    {
        _settings = SettingsStore.Load();
        _clipboardGuardService = new ClipboardGuardService(_settings);

        Text = "ClipGuard";
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        WindowState = FormWindowState.Minimized;
        Opacity = 0;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(-2000, -2000);
        Size = new Size(1, 1);

        _trayMenu = new ContextMenuStrip();

        _monitoringItem = new ToolStripMenuItem("Monitoring active")
        {
            Checked = _settings.EnableMonitoring,
            CheckOnClick = true
        };
        _monitoringItem.CheckedChanged += (_, _) =>
        {
            _settings.EnableMonitoring = _monitoringItem.Checked;
            SettingsStore.Save(_settings);
            _clipboardGuardService.ApplySettings();

            if (_settings.EnableMonitoring)
                _clipboardGuardService.Start();
            else
                _clipboardGuardService.Stop();
        };

        _autoClearItem = new ToolStripMenuItem("Auto-Clear enabled")
        {
            Checked = _settings.EnableAutoClear,
            CheckOnClick = true
        };
        _autoClearItem.CheckedChanged += (_, _) =>
        {
            _settings.EnableAutoClear = _autoClearItem.Checked;
            SettingsStore.Save(_settings);
            _clipboardGuardService.ApplySettings();
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

        var settingsItem = new ToolStripMenuItem("Regex settings");
        settingsItem.Click += (_, _) =>
        {
            using var form = new SettingsForm(_settings);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                SettingsStore.Save(_settings);
                _clipboardGuardService.ApplySettings();
                ShowBalloon("ClipGuard", "Settings saved.", ToolTipIcon.Info);
            }
        };

        var clearItem = new ToolStripMenuItem("Clear the clipboard");
        clearItem.Click += (_, _) =>
        {
            _clipboardGuardService.ClearClipboardManually();
            ShowBalloon("ClipGuard", "Clipboard cleared.", ToolTipIcon.Info);
        };

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Close();

        _trayMenu.Items.Add(_monitoringItem);
        _trayMenu.Items.Add(_autoClearItem);
        _trayMenu.Items.Add(_notificationsItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(settingsItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(clearItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Text = "ClipGuard",
            Visible = true,
            Icon = SystemIcons.Shield,
            ContextMenuStrip = _trayMenu
        };

        _clipboardGuardService.NotificationRequested += (_, e) => ShowBalloon(e.Title, e.Message, e.Icon);
        _clipboardGuardService.StatusRequested += (_, message) => _trayIcon.Text = $"ClipGuard - {message}";

        Load += (_, _) =>
        {
            _clipboardGuardService.Attach(Handle);
            _clipboardGuardService.ApplySettings();

            if (_settings.EnableMonitoring)
                _clipboardGuardService.Start();

            _ready = true;
        };
    }

    protected override void SetVisibleCore(bool value)
    {
        if (!_ready)
        {
            base.SetVisibleCore(false);
            return;
        }

        base.SetVisibleCore(false);
    }

    protected override void WndProc(ref Message m)
    {
        _clipboardGuardService.ProcessWindowMessage(ref m);
        base.WndProc(ref m);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _trayIcon.Visible = false;
        _clipboardGuardService.Dispose();
        _trayIcon.Dispose();
        _trayMenu.Dispose();
        base.OnClosing(e);
    }

    private void ShowBalloon(string title, string message, ToolTipIcon icon)
    {
        if (!_settings.EnableNotifications)
            return;

        _trayIcon.BalloonTipTitle = title;
        _trayIcon.BalloonTipText = message;
        _trayIcon.BalloonTipIcon = icon;
        _trayIcon.ShowBalloonTip(3000);
    }

    private sealed class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private readonly CheckBox _monitoringBox;
        private readonly CheckBox _autoClearBox;
        private readonly CheckBox _notificationsBox;
        private readonly NumericUpDown _autoClearSecondsBox;
        private readonly TextBox _sensitiveBox;
        private readonly TextBox _whitelistBox;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;

            Text = "ClipGuard Regex Settings";
            StartPosition = FormStartPosition.CenterParent;
            Width = 820;
            Height = 620;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var monitoringLabel = new Label
            {
                Text = "General",
                Left = 16,
                Top = 16,
                Width = 200,
                Font = new Font(Font, FontStyle.Bold)
            };

            _monitoringBox = new CheckBox
            {
                Text = "Monitoring active",
                Left = 20,
                Top = 48,
                Width = 200,
                Checked = _settings.EnableMonitoring
            };

            _autoClearBox = new CheckBox
            {
                Text = "Auto-Clear enabled",
                Left = 20,
                Top = 78,
                Width = 200,
                Checked = _settings.EnableAutoClear
            };

            _notificationsBox = new CheckBox
            {
                Text = "View notifications",
                Left = 20,
                Top = 108,
                Width = 240,
                Checked = _settings.EnableNotifications
            };

            var secondsLabel = new Label
            {
                Text = "Auto-Clear Seconds",
                Left = 20,
                Top = 145,
                Width = 180
            };

            _autoClearSecondsBox = new NumericUpDown
            {
                Left = 210,
                Top = 140,
                Width = 100,
                Minimum = 1,
                Maximum = 3600,
                Value = Math.Max(1, _settings.AutoClearSeconds)
            };

            var sensitiveLabel = new Label
            {
                Text = "Sensitive Regex Patterns",
                Left = 16,
                Top = 190,
                Width = 240,
                Font = new Font(Font, FontStyle.Bold)
            };

            _sensitiveBox = new TextBox
            {
                Left = 16,
                Top = 220,
                Width = 360,
                Height = 280,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = false,
                Text = string.Join(Environment.NewLine, _settings.SensitivePatterns)
            };

            var whitelistLabel = new Label
            {
                Text = "Whitelist Regex Patterns",
                Left = 400,
                Top = 190,
                Width = 240,
                Font = new Font(Font, FontStyle.Bold)
            };

            _whitelistBox = new TextBox
            {
                Left = 400,
                Top = 220,
                Width = 360,
                Height = 280,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = false,
                Text = string.Join(Environment.NewLine, _settings.WhitelistPatterns)
            };

            var saveButton = new Button
            {
                Text = "Save",
                Left = 580,
                Top = 520,
                Width = 130,
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Left = 440,
                Top = 520,
                Width = 130,
                DialogResult = DialogResult.Cancel
            };

            saveButton.Click += (_, _) =>
            {
                _settings.EnableMonitoring = _monitoringBox.Checked;
                _settings.EnableAutoClear = _autoClearBox.Checked;
                _settings.EnableNotifications = _notificationsBox.Checked;
                _settings.AutoClearSeconds = (int)_autoClearSecondsBox.Value;

                _settings.SensitivePatterns = _sensitiveBox
                    .Lines
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                _settings.WhitelistPatterns = _whitelistBox
                    .Lines
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(monitoringLabel);
            Controls.Add(_monitoringBox);
            Controls.Add(_autoClearBox);
            Controls.Add(_notificationsBox);
            Controls.Add(secondsLabel);
            Controls.Add(_autoClearSecondsBox);
            Controls.Add(sensitiveLabel);
            Controls.Add(_sensitiveBox);
            Controls.Add(whitelistLabel);
            Controls.Add(_whitelistBox);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }
    }
}