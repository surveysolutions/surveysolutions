using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.ServicesIntegration.Export;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class SpeedByInterviewersReportInputModel: PeriodicReportInputModelBase
    {
        public SpeedByInterviewersReportInputModel()
        {
            this.InterviewStatuses = new InterviewExportedAction[0];
        }

        public Guid SupervisorId { get; set; }
        public string Period { get; set; }
        public InterviewExportedAction[] InterviewStatuses { get; set; }
        public PeriodiceReportType ReportType { get; set; }
    }

    public class InterviewDuractionByInterviewersReportInputModel : PeriodicReportInputModelBase
    {
        public string Period { get; set; }
        public Guid SupervisorId { get; set; }
    }
}
