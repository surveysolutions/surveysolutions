namespace Core.Supervisor.Views.Summary
{
    using System;
    public class SummaryInputModel : ListViewModelBase
    {
        public SummaryInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        public ViewerStatus ViewerStatus { get; set; }

        public Guid ViewerId { get; set; }

        public Guid? TemplateId { get; set; }
    }
}
