using System.Runtime.InteropServices;

public static class MonitorHelper
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfoEx
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // Die Liste privat und statisch deklarieren
    private static List<MonitorInfoEx> monitors;

    // Methode zum Füllen der Liste, wenn sie noch nicht befüllt wurde
    private static void InitializeMonitors()
    {
        if (monitors == null)
        {
            monitors = new List<MonitorInfoEx>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
                {
                    MonitorInfoEx mi = new MonitorInfoEx();
                    mi.cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        monitors.Add(mi);
                    }
                    return true;
                }, IntPtr.Zero);
        }
    }

    // Methode zum Abrufen eines Wertes, um Wiederholungen zu vermeiden
    private static double GetMonitorValue(int v, Func<MonitorInfoEx, double> valueSelector)
    {
        InitializeMonitors(); // Monitore initialisieren, falls noch nicht geschehen
        return valueSelector(monitors[v - 1]);
    }

    // Getter-Methoden
    public static double GetSystemMonitorLeft(int v) => GetMonitorValue(v, m => m.rcMonitor.Left);
    public static double GetSystemMonitorTop(int v) => GetMonitorValue(v, m => m.rcMonitor.Top);
    public static double GetSystemMonitorRight(int v) => GetMonitorValue(v, m => m.rcMonitor.Right);
    public static double GetSystemMonitorBottom(int v) => GetMonitorValue(v, m => m.rcMonitor.Bottom);
    public static double GetSystemMonitorWidth(int v) => GetMonitorValue(v, m => m.rcMonitor.Right - m.rcMonitor.Left);
    public static double GetSystemMonitorHeight(int v) => GetMonitorValue(v, m => m.rcMonitor.Bottom - m.rcMonitor.Top);
}
