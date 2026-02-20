using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

class NightShift : Form
{
    [DllImport("user32.dll")]
    static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

    [DllImport("user32.dll")]
    static extern bool SetProcessDPIAware();

    [DllImport("shcore.dll")]
    static extern int SetProcessDpiAwareness(int value);

    [DllImport("kernel32.dll")]
    static extern uint SetThreadExecutionState(uint esFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

    [DllImport("user32.dll")]
    static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

    const uint ES_CONTINUOUS      = 0x80000000;
    const uint ES_SYSTEM_REQUIRED = 0x00000001;

    const int WM_POWERBROADCAST = 0x0218;
    const int PBT_POWERSETTINGCHANGE = 0x8013;

    static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");

    const string APP_NAME = "NightShift";
    const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    [StructLayout(LayoutKind.Sequential)]
    struct POWERBROADCAST_SETTING
    {
        public Guid PowerSetting;
        public uint DataLength;
        public byte Data;
    }

    private NotifyIcon trayIcon;
    private MenuItem keepAwakeItem;
    private MenuItem startupItem;
    private bool keepAwake = false;
    private IntPtr powerNotificationHandle = IntPtr.Zero;
    private DateTime? monitorsOffTime = null;

    public NightShift()
    {
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        FormBorderStyle = FormBorderStyle.None;
        Opacity = 0;

        trayIcon = new NotifyIcon();
        trayIcon.Icon = LoadIcon();
        trayIcon.Text = "Night Shift";
        this.Icon = trayIcon.Icon;
        trayIcon.Visible = true;

        keepAwakeItem = new MenuItem("Keep System Awake", OnToggleAwake);
        startupItem = new MenuItem("Run on Startup", OnToggleStartup);
        startupItem.Checked = IsStartupEnabled();

        var menu = new ContextMenu(new MenuItem[] {
            new MenuItem("Night Shift") { Enabled = false },
            new MenuItem("-"),
            new MenuItem("Turn Off Monitors", OnMonitorsOff),
            new MenuItem("-"),
            keepAwakeItem,
            new MenuItem("-"),
            new MenuItem("Monitors Off + Stay Awake", OnBoth),
            new MenuItem("-"),
            startupItem,
            new MenuItem("-"),
            new MenuItem("Exit", OnExit)
        });

        trayIcon.ContextMenu = menu;

        trayIcon.MouseClick += delegate(object s, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                ScheduleMonitorsOff();
        };

        trayIcon.BalloonTipTitle = "Night Shift";
        trayIcon.BalloonTipText = "Left-click: monitors off\nRight-click: options";
        trayIcon.BalloonTipIcon = ToolTipIcon.Info;
        trayIcon.ShowBalloonTip(3000);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        powerNotificationHandle = RegisterPowerSettingNotification(
            this.Handle, ref GUID_CONSOLE_DISPLAY_STATE, 0);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (powerNotificationHandle != IntPtr.Zero)
        {
            UnregisterPowerSettingNotification(powerNotificationHandle);
            powerNotificationHandle = IntPtr.Zero;
        }
        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
        {
            var setting = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(
                m.LParam, typeof(POWERBROADCAST_SETTING));

            if (setting.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
            {
                int displayState = setting.Data;

                if (displayState == 0) // Display off
                {
                    monitorsOffTime = DateTime.Now;
                }
                else if (displayState == 1 && monitorsOffTime.HasValue) // Display on
                {
                    var offTime = monitorsOffTime.Value;
                    var duration = DateTime.Now - offTime;
                    monitorsOffTime = null;

                    if (duration.TotalSeconds >= 5)
                    {
                        ShowStatsPopup(offTime, DateTime.Now, duration);
                    }
                }
            }
        }
        base.WndProc(ref m);
    }

    void ShowStatsPopup(DateTime offTime, DateTime onTime, TimeSpan duration)
    {
        var stats = new StatsForm(offTime, onTime, duration);
        stats.Show();
    }

    void OnMonitorsOff(object s, EventArgs e)
    {
        ScheduleMonitorsOff();
    }

    void OnToggleAwake(object s, EventArgs e)
    {
        keepAwake = !keepAwake;
        keepAwakeItem.Checked = keepAwake;
        if (keepAwake)
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);
        else
            SetThreadExecutionState(ES_CONTINUOUS);
        trayIcon.Text = keepAwake ? "Night Shift (Awake)" : "Night Shift";
    }

    void OnToggleStartup(object s, EventArgs e)
    {
        if (IsStartupEnabled())
        {
            RemoveStartup();
            startupItem.Checked = false;
        }
        else
        {
            SetStartup();
            startupItem.Checked = true;
        }
    }

    bool IsStartupEnabled()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
        {
            return key != null && key.GetValue(APP_NAME) != null;
        }
    }

    void SetStartup()
    {
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
        {
            key.SetValue(APP_NAME, "\"" + exePath + "\"");
        }
    }

    void RemoveStartup()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
        {
            if (key != null)
                key.DeleteValue(APP_NAME, false);
        }
    }

    void OnBoth(object s, EventArgs e)
    {
        keepAwake = true;
        keepAwakeItem.Checked = true;
        SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);
        trayIcon.Text = "Night Shift (Awake)";
        ScheduleMonitorsOff();
    }

    void OnExit(object s, EventArgs e)
    {
        SetThreadExecutionState(ES_CONTINUOUS);
        trayIcon.Visible = false;
        Application.Exit();
    }

    void ScheduleMonitorsOff()
    {
        var timer = new Timer();
        timer.Interval = 500;
        timer.Tick += delegate {
            timer.Stop();
            timer.Dispose();
            SendMessage(-1, 0x0112, 0xF170, 2);
        };
        timer.Start();
    }

    static Icon LoadIcon()
    {
        // Try to load the .ico file from next to the exe
        string exeDir = Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location);
        string icoPath = Path.Combine(exeDir, "nightshift.ico");

        if (File.Exists(icoPath))
            return new Icon(icoPath, 32, 32);

        // Fallback: generate a simple moon icon
        var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using (var brush = new SolidBrush(Color.FromArgb(220, 200, 120)))
                g.FillEllipse(brush, 4, 4, 24, 24);

            using (var brush = new SolidBrush(Color.FromArgb(30, 30, 60)))
                g.FillEllipse(brush, 12, 2, 22, 22);

            using (var brush = new SolidBrush(Color.FromArgb(200, 220, 200, 120)))
            {
                g.FillEllipse(brush, 20, 22, 3, 3);
                g.FillEllipse(brush, 26, 14, 2, 2);
            }
        }
        return Icon.FromHandle(bmp.GetHicon());
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    [STAThread]
    static void Main()
    {
        // Enable high-DPI awareness (try per-monitor first, fall back to system)
        try { SetProcessDpiAwareness(2); } // 2 = Per-Monitor DPI Aware
        catch { SetProcessDPIAware(); }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new NightShift());
    }
}

class StatsForm : Form
{
    private Timer fadeTimer;
    private Timer dismissTimer;
    private float opacity = 0f;
    private bool fadingIn = true;

    const int WS_EX_NOACTIVATE = 0x08000000;
    const int WS_EX_TOPMOST = 0x00000008;
    const int WS_EX_TOOLWINDOW = 0x00000080;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    protected override bool ShowWithoutActivation { get { return true; } }

    public StatsForm(DateTime offTime, DateTime onTime, TimeSpan duration)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.FromArgb(30, 30, 46);
        Size = new Size(300, 140);

        var screen = Screen.PrimaryScreen.WorkingArea;
        Location = new Point(screen.Right - Width - 16, screen.Bottom - Height - 16);

        Opacity = 0;

        string durationText = FormatDuration(duration);

        var titleLabel = new Label
        {
            Text = "Monitors were off for",
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = Color.FromArgb(160, 160, 180),
            AutoSize = false,
            Size = new Size(Width - 32, 22),
            Location = new Point(16, 16),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var durationLabel = new Label
        {
            Text = durationText,
            Font = new Font("Segoe UI", 20f, FontStyle.Bold),
            ForeColor = Color.FromArgb(108, 92, 231),
            AutoSize = false,
            Size = new Size(Width - 32, 40),
            Location = new Point(16, 38),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var timesLabel = new Label
        {
            Text = offTime.ToString("h:mm tt") + "  \u2192  " + onTime.ToString("h:mm tt"),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(140, 140, 160),
            AutoSize = false,
            Size = new Size(Width - 32, 20),
            Location = new Point(16, 84),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var appLabel = new Label
        {
            Text = "Night Shift",
            Font = new Font("Segoe UI", 8f, FontStyle.Regular),
            ForeColor = Color.FromArgb(80, 80, 100),
            AutoSize = false,
            Size = new Size(Width - 32, 16),
            Location = new Point(16, 110),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Controls.Add(titleLabel);
        Controls.Add(durationLabel);
        Controls.Add(timesLabel);
        Controls.Add(appLabel);

        // Click anywhere to dismiss
        Click += (s, e) => Close();
        foreach (Control c in Controls)
            c.Click += (s, e) => Close();

        // Rounded corners
        var path = new GraphicsPath();
        int radius = 12;
        path.AddArc(0, 0, radius, radius, 180, 90);
        path.AddArc(Width - radius, 0, radius, radius, 270, 90);
        path.AddArc(Width - radius, Height - radius, radius, radius, 0, 90);
        path.AddArc(0, Height - radius, radius, radius, 90, 90);
        path.CloseFigure();
        Region = new Region(path);

        // Fade in
        fadeTimer = new Timer { Interval = 16 };
        fadeTimer.Tick += OnFadeTick;
        fadeTimer.Start();

        // Auto-dismiss after 8 seconds
        dismissTimer = new Timer { Interval = 8000 };
        dismissTimer.Tick += (s, e) =>
        {
            dismissTimer.Stop();
            fadingIn = false;
            fadeTimer.Start();
        };
    }

    void OnFadeTick(object s, EventArgs e)
    {
        if (fadingIn)
        {
            opacity += 0.08f;
            if (opacity >= 1f)
            {
                opacity = 1f;
                fadeTimer.Stop();
                dismissTimer.Start();
            }
        }
        else
        {
            opacity -= 0.06f;
            if (opacity <= 0f)
            {
                opacity = 0f;
                fadeTimer.Stop();
                Close();
                return;
            }
        }
        Opacity = opacity;
    }

    string FormatDuration(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return string.Format("{0}h {1}m", (int)ts.TotalHours, ts.Minutes);
        if (ts.TotalMinutes >= 1)
            return string.Format("{0}m {1}s", (int)ts.TotalMinutes, ts.Seconds);
        return string.Format("{0}s", (int)ts.TotalSeconds);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Subtle border
        using (var pen = new Pen(Color.FromArgb(50, 108, 92, 231), 1f))
        {
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var path = new GraphicsPath();
            int radius = 12;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(pen, path);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (fadeTimer != null) { fadeTimer.Stop(); fadeTimer.Dispose(); }
            if (dismissTimer != null) { dismissTimer.Stop(); dismissTimer.Dispose(); }
        }
        base.Dispose(disposing);
    }
}
