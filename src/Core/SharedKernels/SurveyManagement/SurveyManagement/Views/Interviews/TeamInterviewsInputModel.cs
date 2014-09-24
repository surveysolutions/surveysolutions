using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class TeamInterviewsInputModel: ListViewModelBase
    {
        public TeamInterviewsInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid? QuestionnaireId { get; set; }

        public long? QuestionnaireVersion { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }

        public Guid ViewerId { get; set; }

        public string SearchBy { get; set; }
    }
}
