using System;

namespace WB.Services.Export
{
    public class SimpleRunningAverage
    {
        readonly int size;
        readonly double[] values;

        int valuesIndex;
        int valueCount;
        double sum;

        public SimpleRunningAverage(int size)
        {
            this.size = Math.Max(size, 1);
            values = new double[this.size];
            valuesIndex = 0;
            valueCount = 0;
            sum = 0;
        }

        public double Average { get; private set; }

        public double Add(double newValue)
        {
            // calculate new value to add to sum by subtracting the 
            // value that is replaced from the new value; 
            var temp = newValue - values[valuesIndex];
            values[valuesIndex] = newValue;
            sum += temp;

            valuesIndex++;
            valuesIndex %= size;

            if (valueCount < size)
                valueCount++;

            Average = sum / valueCount;
            return Average;
        }

        /// <summary>
        /// Return Estimated Time to process {total} amount of items with this average speed per second
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public TimeSpan Eta(long total)
        {
            return TimeSpan.FromSeconds(total / Average);
        }
    }
}