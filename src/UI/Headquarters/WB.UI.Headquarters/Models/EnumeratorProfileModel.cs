using System;

namespace WB.UI.Headquarters.Models
{
    public class EnumeratorProfileModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string LoginName { get; set; }
        public string SupervisorName { get; set; }

        public AssignmentsInfoModel Assignments { get; set; }
    }

    public class AssignmentsInfoModel
    {
        public int NewOnDeviceCount { get; set; }
        public int RejectedCount { get; set; }
        public int WaitingForApprovalCount { get; set; }
        public int ApprovedByHqCount { get; set; }
    }
}