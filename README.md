# Night Shift

<p align="center">
  <img src="logo.png" alt="Night Shift" width="200">
</p>

<p align="center">
  Let your AI agents work through the night while you sleep in a dark room.
</p>

A tiny app that turns off your monitors and keeps your machine awake — built for people running [Claude Code](https://docs.anthropic.com/en/docs/claude-code), [Cowork](https://claude.com/blog/cowork-research-preview), [OpenClaw](https://github.com/anthropics/openclaw), and other AI coding agents that need hours of uninterrupted runtime.

![Windows](https://img.shields.io/badge/platform-Windows-blue)
![macOS](https://img.shields.io/badge/platform-macOS-lightgrey)
![Linux](https://img.shields.io/badge/platform-Linux-orange)
![License](https://img.shields.io/badge/license-MIT-green)

## The Problem

You kick off a long-running AI agent session — a multi-file refactor, an overnight code generation task, a batch of Cowork jobs — and you want to go to sleep. But if your machine goes to sleep, your agents die mid-task. And if you leave the monitors on, your bedroom is lit up like a data center.

**Night Shift** solves both problems: monitors off, machine awake, agents keep running.

## Download

Grab the right file for your OS from the [latest release](../../releases/latest).

| Platform | File | How to run |
|----------|------|------------|
| **Windows** | `NightShift-Setup.exe` | Run the installer |
| **Windows** | `NightShift.exe` | Portable — just double-click |
| **macOS** | `nightshift-mac.command` | Double-click it (may need to right-click > Open the first time) |
| **Linux** | `nightshift-linux.sh` | `chmod +x nightshift-linux.sh && ./nightshift-linux.sh` |

## Usage

### Windows

1. Start your AI agents (Claude Code, OpenClaw, etc.)
2. Run `NightShift.exe` (or install via `NightShift-Setup.exe`)
3. A crescent moon icon appears in your system tray
4. Right-click and hit **Monitors Off + Stay Awake**
5. Go to sleep — your agents keep working, your room stays dark
6. Move your mouse in the morning to wake the monitors
7. A stats popup shows how long your monitors were off
8. Right-click the tray icon and hit **Exit** when you're done

**Tray menu:**
- **Turn Off Monitors** — just the monitors
- **Keep System Awake** — toggleable, prevents sleep
- **Monitors Off + Stay Awake** — the bedtime combo
- **Run on Startup** — toggleable, launches NightShift when Windows starts
- **Exit** — restores normal power settings

**Tip:** Left-click the tray icon anytime to quickly turn monitors off again.

### macOS

1. Start your AI agents
2. Double-click `nightshift-mac.command`
3. Monitors turn off, system stays awake
4. Move your mouse to wake the monitors
5. Press Enter in the terminal window to quit

Uses `caffeinate` (built-in) to prevent sleep and `pmset displaysleepnow` to turn off the display.

### Linux

1. Start your AI agents
2. Run `./nightshift-linux.sh`
3. Monitors turn off, system stays awake
4. Move your mouse to wake the monitors
5. Press Enter in the terminal to quit

Supports X11 (`xset`) and Wayland (`wlopm`, `swaymsg`). Uses `systemd-inhibit` to prevent sleep.

## Features

- **Sleep stats** — when your monitors wake up, a popup shows how long they were off
- **Auto-start** — optionally launch NightShift on Windows startup
- **Installer** — proper Windows installer, shows up in Start Menu and Windows Search
- Zero dependencies — uses built-in OS tools
- Tiny footprint, minimal resource usage
- Monitors wake instantly when you move the mouse or press a key
- Clean exit restores normal power settings

## Building from Source (Windows)

Requires .NET Framework 4.x (included with Windows).

```
csc.exe /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /win32icon:nightshift.ico /out:NightShift.exe NightShift.cs
```

Or using the full path to the compiler:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /win32icon:nightshift.ico /out:NightShift.exe NightShift.cs
```

## License

MIT
