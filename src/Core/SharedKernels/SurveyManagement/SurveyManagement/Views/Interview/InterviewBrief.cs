using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewBrief : IView
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual Guid ResponsibleId { get; set; }
        public virtual InterviewStatus Status { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual bool HasErrors { get; set; }
    }
}