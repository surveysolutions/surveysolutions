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
        public class StopwatchScope : IDisposable
        {
            public string Category { get; }
            public string Name { get; }
            private readonly StopwatchWrapper stopwatch;

            internal StopwatchScope(StopwatchWrapper stopwatch, string category, string name)
            {
                Category = category;
                Name = name;
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
           
            public List<double> Measures { get; } =new List<double>();
            public PercentileValues GetPercentile() => new PercentileValues(this.Measures.OrderBy(m => m).ToList());
            private TimeSpan singleMeasure;

            public long ElapsedMilliseconds => this.stopwatch.ElapsedMilliseconds;

            public TimeSpan ElapsedTimeSpan => this.stopwatch.Elapsed;

            public int MeasuresCount { get; private set; }

            public void Start()
            {
                this.MeasuresCount++;
                singleMeasure = this.stopwatch.Elapsed;
                this.stopwatch.Start();
                
            }

            public void Stop()
            {
                this.stopwatch.Stop();
                Measures.Add(Math.Abs((this.stopwatch.Elapsed - singleMeasure).TotalMilliseconds));
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

                string MsPerMeasure(StopwatchWrapper sww)
                {
                    if (sww.MeasuresCount == 0) return string.Empty;

                    var avg = sww.ElapsedTimeSpan.TotalMilliseconds / sww.MeasuresCount;

                    var percentile = sww.GetPercentile();

                    return $"avg: {avg:F}ms, {percentile.ToStr()}";
                }

                stopwatchesCategory
                    .Value
                    .Stopwatches
                    .OrderByDescending(pair => pair.Value.ElapsedMilliseconds)
                    .Select((pair, index)
                        => $"{index + 1}. {pair.Value.ElapsedTimeSpan:c}, " +
                           $"{pair.Value.MeasuresCount} measures - {pair.Key} " +
                           $"({MsPerMeasure(pair.Value)})")
                    .ForEach(line
                        => detailsBuilder.AppendLine(line));

                detailsBuilder.AppendLine("- - - - - - - - - - - - - - - - - - - - -");
                detailsBuilder.AppendLine();
            }

            detailsBuilder.AppendLine("=====    End of Global Stopwatcher Dump    =====");
            detailsBuilder.AppendLine();

            return detailsBuilder.ToString();
        }

        public static string ToStr(this double value, string format = "0.##")
        {
            return value.ToString(format);
        }

    }

    public class PercentileValues
    {
        /// <summary>
        /// Calculates the Nth percentile from the set of values
        /// </summary>
        /// <remarks>
        /// The implementation is expected to be consitent with the one from Excel.
        /// It's a quite common to export bench output into .csv for further analysis 
        /// And it's a good idea to have same results from all tools being used.
        /// </remarks>
        /// <param name="sortedValues">Sequence of the values to be calculated</param>
        /// <param name="percentile">Value in range 0..100</param>
        /// <returns>Percentile from the set of values</returns>
        // BASEDON: http://stackoverflow.com/a/8137526
        private static double Percentile(List<double> sortedValues, int percentile)
        {
            if (sortedValues == null)
                throw new ArgumentNullException(nameof(sortedValues));
            if (percentile < 0 || percentile > 100)
            {
                throw new ArgumentOutOfRangeException(
                     nameof(percentile), percentile,
                     "The percentile arg should be in range of 0 - 100.");
            }

            if (sortedValues.Count == 0)
                return 0;

            // DONTTOUCH: the following code was taken from http://stackoverflow.com/a/8137526 and it is proven
            // to work in the same way the excel's counterpart does.
            // So it's better to leave it as it is unless you do not want to reimplement it from scratch:)
            double realIndex = percentile / 100.0 * (sortedValues.Count - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;
            if (index + 1 < sortedValues.Count)
                return sortedValues[index] * (1 - frac) + sortedValues[index + 1] * frac;
            else
                return sortedValues[index];
        }

        public double Percentile(int percentile) => Percentile(SortedValues, percentile);

        private List<double> SortedValues { get; }

        public double P0 { get; }
        public double P25 { get; }
        public double P50 { get; }
        public double P67 { get; }
        public double P80 { get; }
        public double P85 { get; }
        public double P90 { get; }
        public double P95 { get; }
        public double P100 { get; }

        internal PercentileValues(List<double> sortedValues)
        {
            SortedValues = sortedValues;

            P0 = Percentile(0);
            P25 = Percentile(25);
            P50 = Percentile(50);
            P67 = Percentile(67);
            P80 = Percentile(80);
            P85 = Percentile(85);
            P90 = Percentile(90);
            P95 = Percentile(95);
            P100 = Percentile(100);
        }

        public string ToStr(bool showLevel = true) => $"[P95: {P95.ToStr()}] [P0: {P0.ToStr()}]; [P50: {P50.ToStr()}]; [P100: {P100.ToStr()}]";
    }
}
