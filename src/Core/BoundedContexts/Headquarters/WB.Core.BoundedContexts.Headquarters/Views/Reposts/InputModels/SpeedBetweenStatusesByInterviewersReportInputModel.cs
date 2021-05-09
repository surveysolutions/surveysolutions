using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.ServicesIntegration.Export;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class SpeedBetweenStatusesByInterviewersReportInputModel : PeriodicReportInputModelBase
    {
        public SpeedBetweenStatusesByInterviewersReportInputModel()
        {
            this.BeginInterviewStatuses = new[] {InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedByHeadquarter};
            this.EndInterviewStatuses = new[] { InterviewExportedAction.Completed };
        }

        public Guid SupervisorId { get; set; }
        public string Period { get; set; }
        public InterviewExportedAction[] BeginInterviewStatuses { get; set; }
        public InterviewExportedAction[] EndInterviewStatuses { get; set; }
    }
}