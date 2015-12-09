using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewReferences : IView
    {
        public Guid InterviewId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public InterviewStatus Status { get; set; }
    }
}