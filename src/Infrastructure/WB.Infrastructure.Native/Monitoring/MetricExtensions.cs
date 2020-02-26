using System.Linq;

namespace WB.Infrastructure.Native.Monitoring
{
    public static class MetricExtensions
    {
        /// <summary>
        /// Get summ of all tracked values based on labels
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="labels">* - for wildcard</param>
        /// <returns></returns>
        public static double GetSummForLabels(this Counter counter, params string[] labels)
        {
            var metricLabels = counter.AllLabels.ToList();
            var summ = 0.0;

            foreach (var label in metricLabels)
            {
                if (label.IsMatchToLabels(labels))
                {
                    summ += counter.Labels(label).Value;
                }
            }

            return summ;
        }

        /// <summary>
        /// Get summ of all tracked values based on labels
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="labels">* - for wildcard</param>
        /// <returns></returns>
        public static double GetSummForLabels(this Gauge counter, params string[] labels)
        {
            var metricLabels = counter.AllLabels.ToList();
            var summ = 0.0;

            foreach (var label in metricLabels)
            {
                if (label.IsMatchToLabels(labels))
                {
                    summ += counter.Labels(label).Value;
                }
            }

            return summ;
        }

        public static double GetDiffForLabels(this Counter counter, string[] labelsA, string[] labelsB)
        {
            var a = counter.GetSummForLabels(labelsA);
            var b = counter.GetSummForLabels(labelsB);
            return a - b;
        }   
        
        public static double GetDiffForLabels(this Gauge counter, string[] labelsA, string[] labelsB)
        {
            var a = counter.GetSummForLabels(labelsA);
            var b = counter.GetSummForLabels(labelsB);
            return a - b;
        }

        private static bool IsMatchToLabels(this string[] a, string[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] == "*") continue;
                if (a[i] != b[i]) return false;
            }

            return true;
        }
    }
}
