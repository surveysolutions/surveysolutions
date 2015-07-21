using System;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class SpeedBetweenStatusesBySupervisorsReportInputModel : ListViewModelBase
    {
        public SpeedBetweenStatusesBySupervisorsReportInputModel()
        {
            this.BeginInterviewStatuses = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedByHeadquarter };
            this.EndInterviewStatuses = new[] { InterviewExportedAction.Completed };
        }

        public DateTime From { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Period { get; set; }
        public int ColumnCount { get; set; }

        public InterviewExportedAction[] BeginInterviewStatuses { get; set; }
        public InterviewExportedAction[] EndInterviewStatuses { get; set; }

        public PeriodiceReportType ReportType { get; set; }
    }
}