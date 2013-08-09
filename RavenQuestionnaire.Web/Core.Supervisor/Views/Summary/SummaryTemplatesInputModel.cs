using System;

using System;

namespace Core.Supervisor.Views.Summary
{
    public class SummaryTemplatesInputModel
    {
        public SummaryTemplatesInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        public Guid ViewerId { get; set; }
        public ViewerStatus ViewerStatus { get; set; }
    }
}