using System;

namespace AtHighlighter.WindowsAPI
{
    public static class Delegates
    {
        public delegate bool MonitorEnumDelegate(
            IntPtr hMonitor,
            IntPtr hdcMonitor,
            ref RECT lprcMonitor,
            IntPtr dwData);
    }
}