using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.ServicesIntegration.Export;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class QuantityBySupervisorsReportInputModel : PeriodicReportInputModelBase
    {
        public QuantityBySupervisorsReportInputModel()
        {
            this.InterviewStatuses = new InterviewExportedAction[0];
        }

        public string Period { get; set; }
        public InterviewExportedAction[] InterviewStatuses { get; set; }
        public PeriodiceReportType ReportType { get; set; }
    }
}
