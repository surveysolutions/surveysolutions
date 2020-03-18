using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class InterviewComment : IReadSideRepositoryEntity
    {
        protected InterviewComment()
        {
            this.RosterVector = Array.Empty<decimal>();
        }

        public InterviewComment(InterviewSummary summary)
        {
            this.InterviewCommentaries = summary;
        }

        public virtual int Id { get; set; }
        public virtual int CommentSequence { get; set; }
        public virtual string OriginatorName { get; set; }
        public virtual Guid OriginatorUserId { get; set; }
        public virtual UserRoles OriginatorRole { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual string Variable { get; set; }
        public virtual string Roster { get; set; }
        public virtual decimal[] RosterVector { get; set; }
        public virtual string Comment { get; set; }

        public virtual InterviewSummary InterviewCommentaries { get; set; }
    }
}
