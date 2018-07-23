namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyStatus
    {
        public bool IsCanceled { get; set; }

        public bool IsInterrupted { get; set; }

        public bool IsSuccess { get; set; }
        public ConnectionStatusCode Status { get; set; }

        public int StatusCode { get; set; }

        public string StatusMessage { get; set; }

        public static NearbyStatus Ok = new NearbyStatus
        {
            IsSuccess = true
        };
    }
}
