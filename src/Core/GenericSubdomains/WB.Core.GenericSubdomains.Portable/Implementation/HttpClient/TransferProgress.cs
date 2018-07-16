using System;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class TransferProgress
    {
        public long? TotalBytesToReceive { get; set; }
        public long BytesReceived { get; set; }
        public decimal ProgressPercentage { get; set; }

        public TimeSpan Eta { get; set; }
        public decimal? Percent { get; set; }
        public double? Speed { get; set; }
    }
}
