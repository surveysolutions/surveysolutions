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
    }
}