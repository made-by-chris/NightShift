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

Grab the right file for your OS from the [latest release](../../releases/latest). No installation needed.

| Platform | File | How to run |
|----------|------|------------|
| **Windows** | `NightShift.exe` | Double-click it |
| **macOS** | `nightshift-mac.command` | Double-click it (may need to right-click > Open the first time) |
| **Linux** | `nightshift-linux.sh` | `chmod +x nightshift-linux.sh && ./nightshift-linux.sh` |

## Usage

### Windows

1. Start your AI agents (Claude Code, OpenClaw, etc.)
2. Run `NightShift.exe`
3. A crescent moon icon appears in your system tray
4. Right-click and hit **Monitors Off + Stay Awake**
5. Go to sleep — your agents keep working, your room stays dark
6. Move your mouse in the morning to wake the monitors
7. Right-click the tray icon and hit **Exit** when you're done

**Tray menu:**
- **Turn Off Monitors** — just the monitors
- **Keep System Awake** — toggleable, prevents sleep
- **Monitors Off + Stay Awake** — the bedtime combo
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

- Zero dependencies — uses built-in OS tools
- Tiny footprint, minimal resource usage
- Monitors wake instantly when you move the mouse or press a key
- Clean exit restores normal power settings

## Building from Source (Windows)

Requires .NET 10 SDK.

On Windows, you can also double-click `build.cmd` for one-click publishing.

```
dotnet run .\NightShift.cs
```

To publish a standalone executable:

```
dotnet publish .\NightShift.cs -r win-x64 -c Release
```

## License

MIT
