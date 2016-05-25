using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewStatuses : IView
    {
        public InterviewStatuses()
        {
            this.InterviewCommentedStatuses = new List<InterviewCommentedStatus>();
        }

        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual IList<InterviewCommentedStatus> InterviewCommentedStatuses { get; set; }
    }
}