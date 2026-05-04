using System.Runtime.InteropServices;

namespace DeepSeekDeskBand;

static class Program
{
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr,
        ref int attrValue, int attrSize);

    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int Left, Top, Right, Bottom; }

    static readonly IntPtr HWND_TOPMOST = new(-1);
    const uint SWP_SHOWWINDOW = 0x0040;
    const uint SWP_NOACTIVATE = 0x0010;
    const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    const int DWMWA_MICA = 1029;

    public static (int right, int top) GetTaskbarAbove()
    {
        var hwnd = FindWindow("Shell_TrayWnd", null);
        if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out var r))
            return (r.Right, r.Top);
        return (Screen.PrimaryScreen!.Bounds.Right,
                Screen.PrimaryScreen.Bounds.Bottom - 48);
    }

    public static void ApplyAcrylic(Form form)
    {
        var v = 1;
        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref v, 4);
        DwmSetWindowAttribute(form.Handle, DWMWA_MICA, ref v, 4);
    }

    public static void RepositionTopmost(Form form)
    {
        SetWindowPos(form.Handle, HWND_TOPMOST, form.Left, form.Top,
            form.Width, form.Height, SWP_SHOWWINDOW | SWP_NOACTIVATE);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        var form = new Form1();
        var (r, t) = GetTaskbarAbove();
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(r - form.Width - 40, t - form.Height - 2);

        form.Load += (s, e) =>
        {
            RepositionTopmost(form);
            ApplyAcrylic(form);
        };

        Application.Run(form);
    }
}
