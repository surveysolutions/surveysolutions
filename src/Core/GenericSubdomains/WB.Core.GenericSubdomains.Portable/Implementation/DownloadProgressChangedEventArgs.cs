namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class DownloadProgressChangedEventArgs
    {
        public long? TotalBytesToReceive { get; set; }
        public long BytesReceived { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
}
