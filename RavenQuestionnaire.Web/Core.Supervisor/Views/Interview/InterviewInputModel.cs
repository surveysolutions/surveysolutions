namespace Core.Supervisor.Views.Interview
{
    using System;

    public class InterviewInputModel: ListViewModelBase
    {
        #region Constructors and Destructors

        public InterviewInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        #endregion

        #region Public Properties

        public Guid? TemplateId { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid? InterviewId { get; set; }

        public Guid? StatusId { get; set; }

        public bool OnlyNotAssigned { get; set; }

        public Guid ViewerId { get; set; }
        public ViewerStatus ViewerStatus { get; set; }

        #endregion
    }
}