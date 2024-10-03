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

    public static List<MonitorInfoEx> GetMonitors()
    {
        InitializeMonitors();
        return monitors;
    }

    // Getter für SystemMonitorLeft
    public static double GetSystemMonitorLeft(int v)
    {
        InitializeMonitors(); // Stelle sicher, dass die Monitore initialisiert sind
        return monitors[v-1].rcMonitor.Left;
    }

    internal static double GetSystemMonitorTop(int v)
    {
        InitializeMonitors(); // Stelle sicher, dass die Monitore initialisiert sind
        return monitors[v-1].rcMonitor.Top;
    }

    internal static double GetSystemMonitorWidth(int v)
    {
        InitializeMonitors(); // Stelle sicher, dass die Monitore initialisiert sind
        return monitors[v-1].rcMonitor.Right;
    }

    internal static double GetSystemMonitorHeight(int v)
    {
        InitializeMonitors(); // Stelle sicher, dass die Monitore initialisiert sind
        return monitors[v-1].rcMonitor.Bottom;
    }

    // Weitere Getter-Methoden (z.B. Top, Width, Height) können hier folgen
}
