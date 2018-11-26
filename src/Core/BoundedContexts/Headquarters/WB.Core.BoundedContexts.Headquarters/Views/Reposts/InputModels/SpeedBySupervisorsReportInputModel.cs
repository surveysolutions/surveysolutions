using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class SpeedBySupervisorsReportInputModel : PeriodicReportInputModelBase
    {
        public SpeedBySupervisorsReportInputModel()
        {
            this.InterviewStatuses = new[] { InterviewExportedAction.Completed };
        }

        public string Period { get; set; }
        public InterviewExportedAction[] InterviewStatuses { get; set; }

        public PeriodiceReportType ReportType { get; set; }
    }
}
