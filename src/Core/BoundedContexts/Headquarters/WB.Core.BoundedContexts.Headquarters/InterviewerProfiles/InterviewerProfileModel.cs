using System;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerProfiles
{
    public class InterviewerProfileModel : InterviewerProfileToExport
    {
        public int WaitingInterviewsForApprovalCount { get; set; }
        public int ApprovedInterviewsByHqCount { get; set; }
        public SynchronizationActivity SynchronizationActivity { get; set; }
        public int RegistredDevicesCount { get; set; }
        public bool HasAnyGpsAnswerForInterviewer { get; set; }

        public bool SupportQRCodeGeneration { set; get; }
        public string QRCodeAsBase64String { set; get; }

    }

    public class InterviewerProfileSyncStatistics
    {
        public DateTime? SyncDate { get; set; }
        public bool HasStatistics { get; set; }
        public TimeSpan TotalSyncDuration { get; set; }
        public double TotalConnectionSpeed { get; set; }
        public string NetworkType { get; set; }
        public string NetworkSubType { get; set; }
        public string MobileOperator { get; set; }
        public long TotalUploadedBytes { get; set; }
        public long TotalDownloadedBytes { get; set; }
    }

    public class InterviewerProfileToExport
    {
        public string InterviewerName { get; set; }
        public Guid InterviewerId { get; set; }
        public string SupervisorName { get; set; }
        public Guid SupervisorId { get; set; }
        public string InterviewerAppVersion { get; set; }
        public bool HasUpdateForInterviewerApp { get; set; }
        public DateTime? DeviceAssignmentDate { get; set; }
        public int TotalNumberOfSuccessSynchronizations { get; set; }
        public int TotalNumberOfFailedSynchronizations { get; set; }
        public double? AverageSyncSpeedBytesPerSecond { get; set; }
        public InterviewerProfileSyncStatistics LastSuccessfulSync { get; set; }
        public InterviewerProfileSyncStatistics LastFailedSync { get; set; }
        public DateTime? LastCommunicationDate { get; set; }
        public string DeviceId { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceBuildNumber { get; set; }
        public string DeviceLanguage { get; set; }
        public string AndroidVersion { get; set; }
        public string SurveySolutionsVersion { get; set; }
        public DateTime? LastSurveySolutionsUpdatedDate { get; set; }
        public string DeviceOrientation { get; set; }
        public int? BatteryStatus { get; set; }
        public string BatteryPowerSource { get; set; }
        public bool? IsPowerSaveMode { get; set; }
        public long? StorageFreeInBytes { get; set; }
        public long? StorageTotalInBytes { get; set; }
        public long? RamFreeInBytes { get; set; }
        public long? RamTotalInBytes { get; set; }
        public long? DatabaseSizeInBytes { get; set; }
        public DateTime? ServerTimeAtTheBeginningOfSync { get; set; }
        public DateTime? TabletTimeAtTeBeginningOfSync { get; set; }
        public string ConnectionType { get; set; }
        public string ConnectionSubType { get; set; }
        public int QuestionnairesReceived { get; set; }
        public int InterviewsReceived { get; set; }
        public int CompletedInterviewsReceivedFromInterviewer { get; set; }
        public int AssignmentsThatHaveBeenStarted { get; set; }
        public bool HasDeviceInfo { get; set; }
        public int NewInterviewsOnDevice { get; set; } = 0;
        public int RejectedInterviewsOnDevice { get; set; } = 0;
        public string Email { get; set; }
        public bool IsArchived { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public double? DeviceLocationOrLastKnownLocationLat { get; set; }
        public double? DeviceLocationOrLastKnownLocationLon { get; set; }
        public long TrafficUsed { get; set; }
    }
}
