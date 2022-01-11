#nullable enable

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class RemoteTabletSettingsApiView
    {
        public RemoteTabletSettingsApiView()
        {
            PartialSynchronizationEnabled = false;
            WebInterviewUrlTemplate = string.Empty;
        }

        public bool PartialSynchronizationEnabled { get; set; }
        public string WebInterviewUrlTemplate { get; set; }
    }
}
