using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

    const uint ES_CONTINUOUS      = 0x80000000;
    const uint ES_SYSTEM_REQUIRED = 0x00000001;

    private NotifyIcon trayIcon;
    private MenuItem keepAwakeItem;
    private bool keepAwake = false;

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

        var menu = new ContextMenu(new MenuItem[] {
            new MenuItem("Night Shift") { Enabled = false },
            new MenuItem("-"),
            new MenuItem("Turn Off Monitors", OnMonitorsOff),
            new MenuItem("-"),
            keepAwakeItem,
            new MenuItem("-"),
            new MenuItem("Monitors Off + Stay Awake", OnBoth),
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
