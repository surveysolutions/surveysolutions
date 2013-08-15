using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Interview
{
    using System;

    public class InterviewInputModel: ListViewModelBase
    {
        public InterviewInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        public Guid? QuestionnaireId { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }

        public Guid ViewerId { get; set; }

        public ViewerStatus ViewerStatus { get; set; }
    }
}