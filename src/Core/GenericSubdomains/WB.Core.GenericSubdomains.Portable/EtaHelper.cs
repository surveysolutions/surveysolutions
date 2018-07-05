using System;
using System.Diagnostics;

namespace WB.Core.GenericSubdomains.Portable
{
    public class EtaTransferRate
    {
        private long? totalBytes;
        private long bytesDone;
        private readonly Stopwatch sw;
        private readonly SimpleRunningAverage average;

        public EtaTransferRate(long? totalBytes = null, int averageWindow = 5)
        {
            this.totalBytes = totalBytes;
            sw = Stopwatch.StartNew();
            this.average = new SimpleRunningAverage(averageWindow);
        }

        public void AddProgress(long sendBytes, long? totalBytes = null)
        {
            this.totalBytes = totalBytes;
            var bytes = sendBytes - bytesDone;
            var elapsed = sw.Elapsed.TotalSeconds;
            var speed = bytes / elapsed; // bytes/second
            
            AverageSpeed = this.average.Add(speed);
            bytesDone = sendBytes;
            ETA = this.totalBytes != null ? TimeSpan.FromSeconds((this.totalBytes.Value - bytesDone) / AverageSpeed) : TimeSpan.Zero;
        }

        public TimeSpan ETA { get; private set; }
        public double AverageSpeed { get; private set; }
    }

    public class EtaHelper
    {
        private long totalItems;
        private readonly long batchSize;
        private readonly Stopwatch trackingStopwatch;
        
        private readonly SimpleRunningAverage average;

        public EtaHelper(long totalItems, long batchSize, Stopwatch trackingStopwatch = null, int averageWindow = 7)
        {
            this.totalItems = totalItems;
            this.batchSize = batchSize;
            this.trackingStopwatch = trackingStopwatch;
            this.average = new SimpleRunningAverage(averageWindow);
            perProgressSpan = TimeSpan.Zero;
        }

        public TimeSpan perProgressSpan;
        public TimeSpan ETA { get; private set; }
        public long ItemsPerHour { get; private set; }

        public void AddProgress(long itemsProcessed)
        {
            AddProgress((trackingStopwatch.Elapsed - perProgressSpan).TotalMilliseconds, itemsProcessed);
            perProgressSpan = trackingStopwatch.Elapsed;
        }

        public TimeSpan AddProgress(double milliseconds, long itemsCount)
        {
            var perBatchSize = Math.Round(milliseconds / batchSize * itemsCount , 0);
            
            // ms * items * {batchSize}
            var avgPerBatchSize = this.average.Add(perBatchSize); // average speed of processing batchSize items count;
            
            ItemsPerHour = (long) Math.Round(batchSize * 3600.0 * 1000.0 / avgPerBatchSize, 2);

            totalItems -= itemsCount;
            ETA = TimeSpan.FromMilliseconds(totalItems * avgPerBatchSize / batchSize);
            
            return ETA;
        }

        public override string ToString() => $"ETA: {ETA:g} ({ItemsPerHour} items/hour)";
    }
}
