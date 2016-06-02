using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class AllInterviewsInputModel: ListViewModelBase
    {
        public Guid? QuestionnaireId { get; set; }

        public string TeamLeadName { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }

        public long? QuestionnaireVersion { get; set; }

        public string SearchBy { get; set; }
    }
}