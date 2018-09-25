using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetInterviewerAppRequest : ICommunicationMessage
    {
        public GetInterviewerAppRequest(int appVersion, EnumeratorApplicationType appType)
        {
            this.AppVersion = appVersion;
            this.AppType = appType;
        }

        public int AppVersion { get; }
        public EnumeratorApplicationType AppType { get; }
    }
    public class GetInterviewerAppResponse : ICommunicationMessage
    {
        public byte[] Content { get; set; }
    }
}
