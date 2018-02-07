using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public class EtaHelper
    {
        private long totalItems;
        private readonly long batchSize;
        private readonly SimpleRunningAverage average;

        public EtaHelper(long totalItems, long batchSize, int averageWindow = 7)
        {
            this.totalItems = totalItems;
            this.batchSize = batchSize;
            this.average = new SimpleRunningAverage(averageWindow);
        }

        public TimeSpan ETA { get; private set; }
        public long ItemsPerHour { get; private set; }

        public TimeSpan AddProgress(double milliseconds, long itemsCount)
        {
            var perBatchSize = Math.Round(milliseconds / batchSize * itemsCount , 0);
            
            // ms * items * {batchSize}
            var avgPerBatchSize = this.average.Add(perBatchSize); // average speed of processing batchSize items count;

            // 200ms took to process 1500 items
            // 60 * 60 * 1000 to process x items
            ItemsPerHour = (long) Math.Round(batchSize * 3600.0 * 1000.0 / avgPerBatchSize, 2);

            totalItems -= itemsCount;
            ETA = TimeSpan.FromMilliseconds(totalItems * avgPerBatchSize / batchSize);
            
            return ETA;
        }
    }

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