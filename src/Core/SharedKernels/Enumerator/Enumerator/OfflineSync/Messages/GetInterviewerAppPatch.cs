using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetInterviewerAppPatchRequest : ICommunicationMessage
    {
        public GetInterviewerAppPatchRequest(int appVersion, EnumeratorApplicationType appType)
        {
            this.AppVersion = appVersion;
            this.AppType = appType;
        }

        public int AppVersion { get; }
        public EnumeratorApplicationType AppType { get; }
    }
    public class GetInterviewerAppPatchResponse : ICommunicationMessage
    {
        public byte[] Content { get; set; }
    }
}
