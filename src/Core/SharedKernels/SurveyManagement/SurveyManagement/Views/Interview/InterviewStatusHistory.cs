using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewStatusHistory : IReadSideRepositoryEntity
    {
        protected InterviewStatusHistory()
        {
            this.StatusChangeHistory = new List<InterviewCommentedStatus>();
        }

        public InterviewStatusHistory(string interviewId) : this()
        {
            this.InterviewId = interviewId;
        }

        public virtual string InterviewId { get; set; }

        public virtual IList<InterviewCommentedStatus> StatusChangeHistory { get; protected set; }

        protected bool Equals(InterviewStatusHistory other)
        {
            return string.Equals(this.InterviewId, other.InterviewId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InterviewStatusHistory)obj);
        }

        public override int GetHashCode()
        {
            return (this.InterviewId != null ? this.InterviewId.GetHashCode() : 0);
        }
    }
}