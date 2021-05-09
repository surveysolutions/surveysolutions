using System;

namespace WB.ServicesIntegration.Export
{
    public class InterviewComment
    {
        public Guid InterviewId { get; set; }
        public int  CommentSequence { get; set; }
        public string OriginatorName { get; set; } = null!;
        public UserRoles OriginatorRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string Variable { get; set; } = null!;
        public string Roster { get; set; } = null!;
        public int[] RosterVector { get; set; } = Array.Empty<int>();
        public string Comment { get; set; } = null!;
        public string InterviewKey { get; set; } = null!;
    }
}
