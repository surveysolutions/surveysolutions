using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class SupervisorSurveysAndStatusesReportInputModel : ListViewModelBase
    {
        public SupervisorSurveysAndStatusesReportInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }

        public Guid? UserId { get; set; }
    }
}
