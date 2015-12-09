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
            private readonly StopwatchWrapper stopwatch;

            internal StopwatchScope(StopwatchWrapper stopwatch)
            {
                this.stopwatch = stopwatch;
                this.stopwatch.Start();
            }

            public void Dispose()
            {
                this.stopwatch.Stop();
            }
        }

        internal class StopwatchWrapper
        {
            private readonly Stopwatch stopwatch = new Stopwatch();

            public long ElapsedMilliseconds
            {
                get { return this.stopwatch.ElapsedMilliseconds; }
            }

            public TimeSpan ElapsedTimeSpan
            {
                get { return this.stopwatch.Elapsed; }
            }

            public int MeasuresCount { get; private set; }

            public void Start()
            {
                this.MeasuresCount++;
                this.stopwatch.Start();
            }

            public void Stop()
            {
                this.stopwatch.Stop();
            }
        }

        private static readonly Dictionary<string, StopwatchWrapper> stopwatches = new Dictionary<string, StopwatchWrapper>();

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
                    .Select((pair, index) => string.Format("{0}. {1}ms, {2} measures - {3}", index + 1, pair.Value.ElapsedMilliseconds, pair.Value.MeasuresCount, pair.Key)));
            lines.Add("=====    End of Global Stopwatcher Dump    =====");

            Debug.WriteLine(string.Join(Environment.NewLine, lines));
        }
    }
}