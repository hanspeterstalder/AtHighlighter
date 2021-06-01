using System;
using Higliter2nd.Internals;
using Higliter2nd.Overlay;
using Overlay.NET.Common;

namespace Higliter2nd
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Log.Register("Console", new ConsoleLog());
            Log.Debug("Start Highligter");

            var controller = new OverlayController();
            controller.Start();
            Console.ReadLine();
        }
    }
}
