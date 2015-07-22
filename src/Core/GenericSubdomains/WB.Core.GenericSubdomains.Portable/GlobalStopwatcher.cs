using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class GlobalStopwatcher
    {
        public class StopwatchScope : IDisposable
        {
            private readonly Stopwatch stopwatch;

            public StopwatchScope(Stopwatch stopwatch)
            {
                this.stopwatch = stopwatch;
                this.stopwatch.Start();
            }

            public void Dispose()
            {
                this.stopwatch.Stop();
            }
        }

        private static readonly Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

        public static StopwatchScope Scope(string name)
        {
            return new StopwatchScope(stopwatches.GetOrAdd(name));
        }

        public static void Reset()
        {
            stopwatches.Clear();
        }

        public static void DumpToDebug()
        {
            var lines = new List<string>();

            lines.Add("=====   Start of Global Stopwatcher Dump   =====");
            lines.AddRange(
                stopwatches
                    .OrderByDescending(pair => pair.Value.ElapsedMilliseconds)
                    .Select((pair, index) => string.Format("{0}. {1}ms - {2}", index + 1, pair.Value.ElapsedMilliseconds, pair.Key)));
            lines.Add("=====    End of Global Stopwatcher Dump    =====");

            Debug.WriteLine(string.Join(Environment.NewLine, lines));
        }
    }
}