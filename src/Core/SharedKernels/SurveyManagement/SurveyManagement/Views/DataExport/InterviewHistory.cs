using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewHistory : IView
    {
        public InterviewHistory()
        {
            this.InterviewActions = new HashSet<InterviewAction>();
        }

        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual ISet<InterviewAction> InterviewActions { get; set; }
    }
}