using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Apposcope_beta
{
    public static class MonitorHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MonitorInfoEx
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static List<MonitorInfoEx> GetMonitors()
        {
            List<MonitorInfoEx> monitors = new List<MonitorInfoEx>();

            bool result = EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
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

            if (!result)
            {
                Debug.WriteLine("EnumDisplayMonitors schlug fehl.");
            }
            return monitors;
        }

    }
}

