using System;
using Overlay.NET.Common;

namespace Higliter2nd.Internals
{
    public class ConsoleLog : ILogger
    {
        public void WriteLine(string line) => Console.WriteLine(line);
    }
}