using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class InterviewComment
    {
        public InterviewComment()
        {
            this.RosterVector = new decimal[0];
        }

        public virtual int CommentSequence { get; set; }
        public virtual string OriginatorName { get; set; }
        public virtual Guid OriginatorUserId { get; set; }
        public virtual UserRoles OriginatorRole { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual string Variable { get; set; }
        public virtual string Roster { get; set; }
        public virtual decimal[] RosterVector { get; set; }
        public virtual string Comment { get; set; }

        public virtual InterviewCommentaries InterviewCommentaries { get; set; }
    }
}
