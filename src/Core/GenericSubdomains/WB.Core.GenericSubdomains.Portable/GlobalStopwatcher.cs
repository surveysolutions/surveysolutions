using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

            public long ElapsedMilliseconds => this.stopwatch.ElapsedMilliseconds;

            public TimeSpan ElapsedTimeSpan => this.stopwatch.Elapsed;

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

        private class StopwatchesCategory
        {
            public Dictionary<string, StopwatchWrapper> Stopwatches { get; } = new Dictionary<string, StopwatchWrapper>();
        }

        private static readonly Dictionary<string, StopwatchesCategory> categories = new Dictionary<string, StopwatchesCategory>();

        public static StopwatchScope Scope(string name) => Scope("$global", name);

        public static StopwatchScope Scope(string category, string name) => new StopwatchScope(GetStopwatch(category, name));

        private static StopwatchWrapper GetStopwatch(string category, string name)
            => categories.GetOrUpdate(category, () => new StopwatchesCategory()).Stopwatches.GetOrUpdate(name, () => new StopwatchWrapper());

        public static void Reset()
        {
            categories.Clear();
        }

        public static void DumpToDebug()
        {
            Debug.WriteLine(GetMeasureDetails());
        }

        public static string GetMeasureDetails()
        {
            var detailsBuilder = new StringBuilder();

            detailsBuilder.AppendLine();
            detailsBuilder.AppendLine("=====   Start of Global Stopwatcher Dump   =====");
            detailsBuilder.AppendLine();

            foreach (var stopwatchesCategory in categories.OrderBy(category => category.Key))
            {
                detailsBuilder.AppendLine($"---     category: {stopwatchesCategory.Key}     ---");

                stopwatchesCategory
                    .Value
                    .Stopwatches
                    .OrderByDescending(pair => pair.Value.ElapsedMilliseconds)
                    .Select((pair, index)
                        => $"{index + 1}. {pair.Value.ElapsedTimeSpan:c}, {pair.Value.MeasuresCount} measures - {pair.Key}")
                    .ForEach(line
                        => detailsBuilder.AppendLine(line));

                detailsBuilder.AppendLine("- - - - - - - - - - - - - - - - - - - - -");
                detailsBuilder.AppendLine();
            }

            detailsBuilder.AppendLine("=====    End of Global Stopwatcher Dump    =====");
            detailsBuilder.AppendLine();

            return detailsBuilder.ToString();
        }
    }
}