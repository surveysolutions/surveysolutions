using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewStatusTimeSpans : IView
    {
        public InterviewStatusTimeSpans()
        {
            this.TimeSpansBetweenStatuses = new HashSet<TimeSpanBetweenStatuses>();
        }

        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual ISet<TimeSpanBetweenStatuses> TimeSpansBetweenStatuses { get; set; }
    }
}