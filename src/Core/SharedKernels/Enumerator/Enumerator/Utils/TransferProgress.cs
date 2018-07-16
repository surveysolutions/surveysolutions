using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class TransferProgress
    {
        public TimeSpan Eta { get; set; }
        public long TotalBytes { get; set; }
        public long TransferedBytes { get; set; }
        public decimal Percent { get; set; }
        public double Speed { get; set; }
    }
}
