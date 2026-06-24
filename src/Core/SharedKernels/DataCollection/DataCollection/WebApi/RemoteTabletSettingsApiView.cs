#nullable enable

using WB.Core.SharedKernels.DataCollection.ValueObjects;

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
            AllowSupervisorChangeAssignmentStatus = true;
            AllowInterviewerChangeAssignmentStatus = true;
            AudioRecordingQuality = AudioRecordingQuality.Mono44kHz;
        }

        public bool NotificationsEnabled { get; set; }
        public bool PartialSynchronizationEnabled { get; set; }
        public string WebInterviewUrlTemplate { get; set; }
        public int GeographyQuestionAccuracyInMeters { get; set; }
        public int GeographyQuestionPeriodInSeconds { get; set; }
        
        public string EsriApiKey { get; set; } = string.Empty;

        public bool AllowSupervisorChangeAssignmentStatus { get; set; }
        public bool AllowInterviewerChangeAssignmentStatus { get; set; }

        public AudioRecordingQuality AudioRecordingQuality { get; set; }
    }
}
