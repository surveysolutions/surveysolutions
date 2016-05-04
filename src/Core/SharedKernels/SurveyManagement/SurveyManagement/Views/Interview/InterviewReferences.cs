using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewReferences : IView
    {
        protected InterviewReferences() {}

        public InterviewReferences(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public Guid InterviewId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
    }
}