using WB.Core.BoundedContexts.Capi.Services;
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

        public string BaseAddress()
        {
            return this.interviewerSettings.GetSyncAddressPoint();
        }
    }
}