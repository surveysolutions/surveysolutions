using System;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public enum EnumeratorApplicationType
    {
        WithMaps = 1,
        WithoutMaps
    }

    public interface IEnumeratorSettings : IRestServiceSettings
    {
        int EventChunkSize { get; }
        Version GetSupportedQuestionnaireContentVersion();

        int GpsReceiveTimeoutSec { get; }
        double GpsDesiredAccuracy { get; }
        bool VibrateOnError { get; }
        
        bool ShowVariables { get; }
        bool ShowLocationOnMap { get; }
        bool ShowAnswerTime { get; }
        long? LastHqSyncTimestamp { get; }
        void SetLastHqSyncTimestamp(long? lastHqSyncTimestamp);
        EnumeratorApplicationType ApplicationType { get; }
        bool Encrypted { get; }
        void SetEncrypted(bool encrypted);
        bool NotificationsEnabled { get; }
        void SetNotifications(bool notificationsEnabled);
        DateTime? LastSync { get; }
        bool? LastSyncSucceeded { get; }
        void MarkSyncStart();
        void MarkSyncSucceeded();
        string LastOpenedMapName { get; }
        void SetLastOpenedMapName(string mapName);
        bool DashboardViewsUpdated { get; }
        void SetDashboardViewsUpdated(bool updated);
        string WebInterviewUriTemplate { get; }
        void SetWebInterviewUrlTemplate(string tabletSettingsWebInterviewUrlTemplate);
        int GeographyQuestionAccuracyInMeters { get; }
        void SetGeographyQuestionAccuracyInMeters(int geographyQuestionAccuracyInMeters);
        int GeographyQuestionPeriodInSeconds { get; }
        void SetGeographyQuestionPeriodInSeconds(int geographyQuestionPeriodInSeconds);
        
        string EsriApiKey { get; }
        void SetEsriApiKey(string esriApiKey);
        
        bool SupportOfflineMaps { get; }

        bool AllowSupervisorChangeAssignmentStatus { get; }
        void SetAllowSupervisorChangeAssignmentStatus(bool allow);

        bool AllowInterviewerChangeAssignmentStatus { get; }
        void SetAllowInterviewerChangeAssignmentStatus(bool allow);

        AudioRecordingQuality AudioRecordingQuality { get; }
        void SetAudioRecordingQuality(AudioRecordingQuality quality);

        AcceptableGpsLocationSource AcceptableGpsLocationSource { get; }
        void SetAcceptableGpsLocationSource(AcceptableGpsLocationSource source);
    }
}
