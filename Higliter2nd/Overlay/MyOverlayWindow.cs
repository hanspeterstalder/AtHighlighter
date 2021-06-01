using System;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Overlay.NET.Common;
using Overlay.NET.Wpf;
using Process.NET.Windows;

using OverlayWindow = Overlay.NET.Wpf.OverlayWindow;

namespace Higliter2nd.Overlay
{
    [RegisterPlugin("Highlighter", "Hanspeter Stalder", "Highlighter", "1.0")]
    public class MyOverlayWindow : WpfOverlayPlugin
    {
        private readonly TickEngine tickEngine = new TickEngine();
        private bool isSetup = false;
        private Polygon polygon;
        private bool isDisposed = false;

        public override void Enable()
        {
            tickEngine.IsTicking = true;
            base.Enable();
        }

        public override void Disable()
        {
            tickEngine.IsTicking = false;
            base.Disable();
        }

        public override void Initialize(IWindow targetWindow)
        {
            base.Initialize(targetWindow);

            OverlayWindow = new OverlayWindow(targetWindow);

            tickEngine.Interval = (1000 / 60).Milliseconds();

            tickEngine.PreTick += OnPreTick;
            tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            // This will only be true if the target window is active
            // (or very recently has been, depends on your update rate)
            if (OverlayWindow.IsVisible)
            {
                OverlayWindow.Update();
            }
        }

        private void OnPreTick(object sender, EventArgs e)
        {
            if (!isSetup)
            {
                SetUp();
                isSetup = true;
            }

            var activated = TargetWindow.IsActivated;
            var visible = OverlayWindow.IsVisible;

            if (!activated && visible)
            {
                OverlayWindow.Hide();
            }
            else if (activated && !visible)
            {
                OverlayWindow.Show();
            }
        }

        public override void Update() => tickEngine.Pulse();

        private void SetUp()
        {
            polygon = new Polygon
            {
                Points = new PointCollection(5)
                {
                    new Point(100, 150),
                    new Point(120, 130),
                    new Point(140, 150),
                    new Point(140, 200),
                    new Point(100, 200)
                },
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                Fill = new RadialGradientBrush(Color.FromRgb(255, 255, 0), Color.FromRgb(255, 0, 255))
            };

            OverlayWindow.Add(polygon);
        }

        // Clear objects
        public override void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            if (IsEnabled)
            {
                Disable();
            }

            OverlayWindow?.Hide();
            OverlayWindow?.Close();
            OverlayWindow = null;
            tickEngine.Stop();
            

            base.Dispose();
            isDisposed = true;
        }

        ~MyOverlayWindow()
        {
            Dispose();
        }

    }
}