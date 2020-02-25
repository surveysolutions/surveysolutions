using System.Linq;

namespace WB.Infrastructure.Native.Monitoring
{
    public static class MetricExtensions
    {
        /// <summary>
        /// 
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
