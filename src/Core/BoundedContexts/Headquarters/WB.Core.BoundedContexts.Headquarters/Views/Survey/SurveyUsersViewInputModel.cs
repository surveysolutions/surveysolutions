using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SurveyUsersViewInputModel
    {
        public SurveyUsersViewInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        public Guid ViewerId { get; set; }
        public ViewerStatus ViewerStatus { get; set; }
    }
}