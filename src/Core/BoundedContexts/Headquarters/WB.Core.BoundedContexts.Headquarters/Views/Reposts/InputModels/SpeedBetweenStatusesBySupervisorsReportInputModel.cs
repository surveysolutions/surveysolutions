using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.ServicesIntegration.Export;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class SpeedBetweenStatusesBySupervisorsReportInputModel : PeriodicReportInputModelBase
    {
        public SpeedBetweenStatusesBySupervisorsReportInputModel()
        {
            this.BeginInterviewStatuses = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedByHeadquarter };
            this.EndInterviewStatuses = new[] { InterviewExportedAction.Completed };
        }

        public string Period { get; set; }

        public InterviewExportedAction[] BeginInterviewStatuses { get; set; }
        public InterviewExportedAction[] EndInterviewStatuses { get; set; }

        public PeriodiceReportType ReportType { get; set; }
    }
}