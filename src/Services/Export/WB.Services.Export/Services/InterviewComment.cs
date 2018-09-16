using System;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Services
{
    public class InterviewComment
    {
        public Guid InterviewId { get; set; }
        public int  CommentSequence { get; set; }
        public string OriginatorName {get; set; }
        public UserRoles OriginatorRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string Variable { get; set; }
        public string Roster { get; set; }
        public int[] RosterVector { get; set; } = Array.Empty<int>();
        public string Comment { get; set; }
    }
}
