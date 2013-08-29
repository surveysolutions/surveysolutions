namespace Core.Supervisor.Views.Status
{
    using System;

    public class StatusViewInputModel : ListViewModelBase
    {
        public StatusViewInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }
      
        public Guid? StatusId { get; set; }

        public Guid ViewerId { get; set; }

        protected ViewerStatus ViewerStatus { get; set; }
    }
}
