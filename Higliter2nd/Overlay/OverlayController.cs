using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Overlay.NET;
using Overlay.NET.Common;
using Process.NET;
using Process.NET.Memory;

namespace Higliter2nd.Overlay
{
    public class OverlayController
    {
        private ProcessSharp processSharp;
        private MyOverlayWindow myOverlayWindow;
        private bool work;

        public void Start()
        {
            Log.Debug(@"Please type the process name of the window you want to attach to, e.g 'notepad.");
            Log.Debug("Note: If there is more than one process found, the first will be used.");

            var processName = Console.ReadLine();
            var process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                Log.Warn($"No process by the name of [{processName}] was found.");
                Log.Warn("Please open one or use a different name and restart the demo.");
                return;
            }

            processSharp = new ProcessSharp(process, MemoryType.Remote);
            myOverlayWindow = new MyOverlayWindow();

            myOverlayWindow.Initialize(processSharp.WindowFactory.MainWindow);
            myOverlayWindow.Enable();

            work = true;

            Log.Debug("Starting update loop (open the process you specified and drag around)");

            myOverlayWindow.OverlayWindow.Draw += OnDraw;

            // Do work
            while (work)
            {
                myOverlayWindow.Update();
            }
        }

        private void OnDraw(object sender, DrawingContext context)
        {
            // Draw a formatted text string into the DrawingContext.
            context.DrawText(
                new FormattedText("Click Me!", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                    new Typeface("Verdana"), 36, Brushes.BlueViolet), new Point(200, 116));

            context.DrawLine(new Pen(Brushes.Blue, 10), new Point(100, 100), new Point(10, 10));
        }
    }
}