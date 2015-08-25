using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AllInterviewsInputModel: ListViewModelBase
    {
        public Guid? QuestionnaireId { get; set; }

        public Guid? TeamLeadId { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }

        public long? QuestionnaireVersion { get; set; }

        public string SearchBy { get; set; }
    }
}