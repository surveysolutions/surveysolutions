namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyConnectionInfo
    {
        public string EndpointName { get; set; }
        public bool IsIncomingConnection { get; set; }
        public string AuthenticationToken { get; set; }
    }
}
