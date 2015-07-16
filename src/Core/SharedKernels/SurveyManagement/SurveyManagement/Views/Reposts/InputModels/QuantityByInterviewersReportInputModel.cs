using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class QuantityByInterviewersReportInputModel : ListViewModelBase
    {
        public QuantityByInterviewersReportInputModel()
        {
            this.InterviewStatuses = new InterviewExportedAction[0];
        }

        public Guid SupervisorId { get; set; }
        public DateTime From { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Period { get; set; }
        public int ColumnCount { get; set; }
        public InterviewExportedAction[] InterviewStatuses { get; set; }
        public PeriodiceReportType ReportType { get; set; }
    }
}