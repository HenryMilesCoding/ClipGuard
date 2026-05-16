using System.Windows.Forms;

namespace ClipGuard;

public sealed class SettingsForm : Form
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

        Text = "ClipGuard Settings";

        Width = 820;
        Height = 620;

        StartPosition = FormStartPosition.CenterScreen;

        FormBorderStyle = FormBorderStyle.FixedDialog;

        MaximizeBox = false;
        MinimizeBox = false;

        // -----------------------------
        // GENERAL
        // -----------------------------

        var generalLabel = new Label
        {
            Text = "General",
            Left = 20,
            Top = 20,
            Width = 200,
            Font = new Font(Font, FontStyle.Bold)
        };

        _monitoringBox = new CheckBox
        {
            Text = "Enable Monitoring",
            Left = 20,
            Top = 55,
            Width = 200,
            Checked = _settings.EnableMonitoring
        };

        _autoClearBox = new CheckBox
        {
            Text = "Enable Auto Clear",
            Left = 20,
            Top = 85,
            Width = 200,
            Checked = _settings.EnableAutoClear
        };

        _notificationsBox = new CheckBox
        {
            Text = "Enable Notifications",
            Left = 20,
            Top = 115,
            Width = 220,
            Checked = _settings.EnableNotifications
        };

        var secondsLabel = new Label
        {
            Text = "Auto Clear Seconds",
            Left = 20,
            Top = 150,
            Width = 180
        };

        _autoClearSecondsBox = new NumericUpDown
        {
            Left = 210,
            Top = 145,
            Width = 100,
            Minimum = 1,
            Maximum = 3600,
            Value = _settings.AutoClearSeconds
        };

        // -----------------------------
        // SENSITIVE
        // -----------------------------

        var sensitiveLabel = new Label
        {
            Text = "Sensitive Regex Patterns",
            Left = 20,
            Top = 210,
            Width = 250,
            Font = new Font(Font, FontStyle.Bold)
        };

        _sensitiveBox = new TextBox
        {
            Left = 20,
            Top = 240,
            Width = 340,
            Height = 280,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = string.Join(Environment.NewLine,
                _settings.SensitivePatterns)
        };

        // -----------------------------
        // WHITELIST
        // -----------------------------

        var whitelistLabel = new Label
        {
            Text = "Whitelist Regex Patterns",
            Left = 420,
            Top = 210,
            Width = 250,
            Font = new Font(Font, FontStyle.Bold)
        };

        _whitelistBox = new TextBox
        {
            Left = 420,
            Top = 240,
            Width = 340,
            Height = 280,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = string.Join(Environment.NewLine,
                _settings.WhitelistPatterns)
        };

        // -----------------------------
        // BUTTONS
        // -----------------------------

        var saveButton = new Button
        {
            Text = "Save",
            Left = 620,
            Top = 540,
            Width = 140
        };

        saveButton.Click += (_, _) =>
        {
            SaveSettings();

            DialogResult = DialogResult.OK;

            Close();
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            Left = 460,
            Top = 540,
            Width = 140
        };

        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;

            Close();
        };

        // -----------------------------
        // ADD CONTROLS
        // -----------------------------

        Controls.Add(generalLabel);

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
    }

    private void SaveSettings()
    {
        _settings.EnableMonitoring =
            _monitoringBox.Checked;

        _settings.EnableAutoClear =
            _autoClearBox.Checked;

        _settings.EnableNotifications =
            _notificationsBox.Checked;

        _settings.AutoClearSeconds =
            (int)_autoClearSecondsBox.Value;

        _settings.SensitivePatterns =
            _sensitiveBox.Lines
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

        _settings.WhitelistPatterns =
            _whitelistBox.Lines
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
    }
}