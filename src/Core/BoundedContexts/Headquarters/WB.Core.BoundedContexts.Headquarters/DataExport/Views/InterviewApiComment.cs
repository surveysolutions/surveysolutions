using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class InterviewApiComment
    {
        public string InterviewId { get; set; }
        public int  CommentSequence { get; set; }
        public string OriginatorName {get; set; }
        public UserRoles OriginatorRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string Variable { get; set; }
        public string Roster { get; set; }
        public decimal[] RosterVector { get; set; }
        public string Comment { get; set; }
    }
}
