using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewStatuses : IView
    {
        public InterviewStatuses()
        {
            this.InterviewCommentedStatuses = new HashSet<InterviewCommentedStatus>();
        }

        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual ISet<InterviewCommentedStatus> InterviewCommentedStatuses { get; set; }
    }
}