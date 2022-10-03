#nullable enable

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class RemoteTabletSettingsApiView
    {
        public RemoteTabletSettingsApiView()
        {
            NotificationsEnabled = false;
            PartialSynchronizationEnabled = false;
            WebInterviewUrlTemplate = string.Empty;
            GeographyQuestionAccuracyInMeters = 10;
            GeographyQuestionPeriodInSeconds = 10;
        }

        public bool NotificationsEnabled { get; set; }
        public bool PartialSynchronizationEnabled { get; set; }
        public string WebInterviewUrlTemplate { get; set; }
        public int GeographyQuestionAccuracyInMeters { get; set; }
        public int GeographyQuestionPeriodInSeconds { get; set; }
    }
}
