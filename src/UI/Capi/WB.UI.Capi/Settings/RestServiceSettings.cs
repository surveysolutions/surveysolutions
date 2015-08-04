using System;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Capi.Settings
{
    public class RestServiceSettings : IRestServiceSettings
    {
        private readonly IInterviewerSettings interviewerSettings;

        public RestServiceSettings(IInterviewerSettings interviewerSettings)
        {
            this.interviewerSettings = interviewerSettings;
        }

        public string Endpoint
        {
            get { return this.interviewerSettings.GetSyncAddressPoint(); }
        }

        public TimeSpan Timeout
        {
            get { return new TimeSpan(0, 0, 0, 30); }
        }

        public int BufferSize
        {
            get { return 512; }
        }

        public bool AcceptUnsignedSslCertificate
        {
            get { return false; }
        }
    }
}