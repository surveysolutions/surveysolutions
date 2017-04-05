using System;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.UI.Headquarters.Models
{
    public class InterviewerProfileModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string LoginName { get; set; }
        public string SupervisorName { get; set; }
        public bool HasUpdateForInterviewerApp { get; set; }
        public int WaitingInterviewsForApprovalCount { get; set; }
        public int ApprovedInterviewsByHqCount { get; set; }
        public DeviceSyncInfo LastSuccessDeviceInfo { get; set; }
        public DeviceSyncInfo LastFailedDeviceInfo { get; set; }
        public int TotalSuccessSynchronizationCount { get; set; }
        public int TotalFailedSynchronizationCount { get; set; }
    }
}