using System;
using WB.Core.GenericSubdomains.Utils.Services;

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
            set { throw new NotImplementedException(); }
        }

        public TimeSpan Timeout
        {
            get { return new TimeSpan(0, 0, 0, 30); }
            set { throw new NotImplementedException(); }
        }

        public int BufferSize
        {
            get { return 512; }
            set { throw new NotImplementedException(); }
        }
    }
}