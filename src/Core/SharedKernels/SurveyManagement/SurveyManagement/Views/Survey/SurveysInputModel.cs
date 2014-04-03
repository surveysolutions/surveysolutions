using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Survey
{
    public class SurveysInputModel : ListViewModelBase
    {
        public SurveysInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        public ViewerStatus ViewerStatus { get; set; }

        /// <summary>
        ///     Gets or sets ViewerId.
        /// </summary>
        public Guid ViewerId { get; set; }

        /// <summary>
        ///     Gets or sets UserId.
        /// </summary>
        public Guid? UserId { get; set; }
    }
}