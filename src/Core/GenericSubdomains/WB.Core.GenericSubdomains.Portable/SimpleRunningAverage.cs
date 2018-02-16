using System;

namespace WB.Core.GenericSubdomains.Portable
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

            return sum / valueCount;
        }
    }
}