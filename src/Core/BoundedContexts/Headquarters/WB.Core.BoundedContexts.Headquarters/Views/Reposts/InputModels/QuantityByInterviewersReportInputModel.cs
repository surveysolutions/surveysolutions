using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.ServicesIntegration.Export;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class QuantityByInterviewersReportInputModel : PeriodicReportInputModelBase
    {
        public QuantityByInterviewersReportInputModel()
        {
            this.InterviewStatuses = new InterviewExportedAction[0];
        }

        public Guid SupervisorId { get; set; }
        public string Period { get; set; }
        public InterviewExportedAction[] InterviewStatuses { get; set; }
        public PeriodiceReportType ReportType { get; set; }
    }
}