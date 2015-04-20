using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewStatusHistory : IReadSideRepositoryEntity
    {
        public InterviewStatusHistory()
        {
            this.StatusChangeHistory = new List<InterviewCommentedStatus>();
        }

        public virtual IList<InterviewCommentedStatus> StatusChangeHistory { get; protected set; }
    }
}