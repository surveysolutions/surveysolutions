using System;

namespace WB.ServicesIntegration.Export
{
    public class InterviewAction
    {
        public Guid InterviewId { get; set; }
        public string Key { get; set; } = String.Empty;
        public InterviewExportedAction Status { get; set; }
        public string StatusChangeOriginatorName { get; set; } = String.Empty;
        public UserRoles StatusChangeOriginatorRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string SupervisorName { get; set; } = String.Empty;
        public string InterviewerName { get; set; } = String.Empty;
    }
}
