# ClipGuard

Lightweight clipboard privacy protection for Windows 10 and Windows 11.

ClipGuard automatically monitors your clipboard and removes sensitive content such as passwords, API keys, bearer tokens, and other confidential data before it can remain in clipboard history.

Designed as a small, fast, open-source utility built with .NET 8 and WinForms.

---

# Features

- Real-time clipboard monitoring
- Automatic removal of sensitive content
- Regex-based detection engine
- Configurable whitelist support
- Tray icon integration
- Balloon notifications
- Automatic clipboard cleanup timer
- JSON-based settings persistence
- Lightweight and portable
- No telemetry
- No cloud dependency

---

# Screenshots

_Add screenshots here later._

---

# Supported Platforms

- Windows 10
- Windows 11

---

# Why ClipGuard?

Windows clipboard history (`Win + V`) is convenient, but sensitive information can accidentally remain accessible for hours or even days.

Examples:
- Passwords
- API keys
- Access tokens
- Bearer tokens
- Secrets copied from terminals
- Connection strings
- Internal credentials

ClipGuard helps reduce that risk automatically.

---

# Technology Stack

- .NET 8
- WinForms
- Native Windows Clipboard API
- JSON configuration storage

---

# Project Structure

```text
ClipGuard/
 ├─ MainForm.cs
 ├─ AppSettings.cs
 ├─ SettingsStore.cs
 ├─ ClipboardGuardService.cs
 ├─ PatternMatcher.cs
 └─ Program.cs
```

---

# Installation

## Requirements

- Windows 10 or Windows 11
- .NET 8 Runtime (unless using self-contained release)

---

# Build From Source

```bash
git clone https://github.com/YOUR_USERNAME/ClipGuard.git
cd ClipGuard
dotnet build
```

---

# Run

```bash
dotnet run
```

---

# Publish Portable EXE

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Published files can be found inside:

```text
bin/Release/net8.0-windows/win-x64/publish
```

---

# Configuration

Settings are automatically stored inside:

```text
%AppData%\ClipGuard\settings.json
```

Example configuration:

```json
{
  "EnableMonitoring": true,
  "EnableAutoClear": true,
  "AutoClearSeconds": 10,
  "EnableNotifications": true,
  "SensitivePatterns": [
    "password",
    "token",
    "api[-_ ]?key"
  ],
  "WhitelistPatterns": [
    "localhost",
    "example"
  ]
}
```

---

# Sensitive Pattern Examples

Default detection patterns include:

```regex
password
passwd
token
api[-_ ]?key
bearer\s+[A-Za-z0-9\-\._~\+/]+=*
```

---

# Roadmap

Planned future features:

- Entropy-based token detection
- Secure clipboard vault
- Clipboard history buffering
- Hotkeys
- Startup integration
- NativeAOT publishing
- Logging
- Export/import settings
- Dark mode
- Multi-language support

---

# Security Notes

ClipGuard is intended to reduce accidental exposure of sensitive clipboard data.

It does not:
- encrypt clipboard contents
- sandbox applications
- replace enterprise DLP solutions
- guarantee full protection against malware

---

# Contributing

Pull requests, issues, and feature suggestions are welcome.

If you find a bug or have an idea for improvement, feel free to open an issue.

---

# License

MIT License

---

# Disclaimer

This software is provided "as is", without warranty of any kind.

Use at your own risk.
