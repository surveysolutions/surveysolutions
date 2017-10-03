using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class ThreadHelper
    {
        public static int MainThreadId { get; private set; }

        public static void Initialize(int mainThreadId)
        {
            MainThreadId = mainThreadId;
        }

        public static bool IsOnMainThread => Environment.CurrentManagedThreadId == MainThreadId;
    }

    public static class GlobalStopwatcher
    {
        private const string DebugTag = "WBDEBUG";

        public class StopwatchScope : IDisposable
        {
            public string Category { get; }
            public string Name { get; }
            private readonly StopwatchWrapper stopwatch;

            internal StopwatchScope(StopwatchWrapper stopwatch, string category, string name)
            {
                Debug.WriteLine($"[{(ThreadHelper.IsOnMainThread ? "MT" : "  ")}] Start scope {category} => {name}",
                    DebugTag);
                Category = category;
                Name = name;
                this.stopwatch = stopwatch;
                this.stopwatch.Start();
            }

            public void Dispose()
            {
                this.stopwatch.Stop();
                Debug.WriteLine($"[{(ThreadHelper.IsOnMainThread ? "MT" : "  ")}] Stop scope {Category} => {Name}",
                    DebugTag);
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
            public ConcurrentDictionary<string, StopwatchWrapper> Stopwatches { get; } = new ConcurrentDictionary<string, StopwatchWrapper>();
        }

        private static readonly ConcurrentDictionary<string, StopwatchesCategory> Categories = new ConcurrentDictionary<string, StopwatchesCategory>();

        public static StopwatchScope Scope(string name) => Scope("$global", name);

        public static StopwatchScope Scope(string category, string name)
        {
            return new StopwatchScope(GetStopwatch(category, name), category, name);
        }

        private static StopwatchWrapper GetStopwatch(string category, string name)
            => Categories.GetOrAdd(category, 
                _ => new StopwatchesCategory()).Stopwatches.GetOrAdd(name, _ => new StopwatchWrapper());

        public static void Reset()
        {
            Categories.Clear();
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

            foreach (var stopwatchesCategory in Categories.OrderBy(category => category.Key))
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