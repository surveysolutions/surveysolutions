using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Interview
{
    using System;

    public class AllInterviewsInputModel: ListViewModelBase
    {
        public Guid? QuestionnaireId { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }
    }
}