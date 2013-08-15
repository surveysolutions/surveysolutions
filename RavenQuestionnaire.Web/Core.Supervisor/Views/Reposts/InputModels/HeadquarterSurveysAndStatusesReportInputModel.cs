using System;

namespace Core.Supervisor.Views.Reposts.InputModels
{
    public class HeadquarterSurveysAndStatusesReportInputModel : ListViewModelBase
    {
        public HeadquarterSurveysAndStatusesReportInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }

        public Guid? UserId { get; set; }
    }
}