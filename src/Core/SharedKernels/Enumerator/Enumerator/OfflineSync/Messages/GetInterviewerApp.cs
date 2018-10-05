using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetInterviewerAppRequest : IChunkedByteArrayRequest
    {
        public GetInterviewerAppRequest(int appVersion, EnumeratorApplicationType appType)
        {
            this.AppVersion = appVersion;
            this.AppType = appType;
        }

        public int AppVersion { get; }
        public EnumeratorApplicationType AppType { get; }
        public long Maximum { get; set; }
        public long Skip { get; set; }
    }
    
    public class GetInterviewerAppResponse : IChunkedByteArrayResponse
    {
        public byte[] Content { get; set; }
        public long Skipped { get; set; }
        public int Length { get; set; }
        public long Total { get; set; }
    }
}
