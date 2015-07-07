using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class QuantityByInterviewersReportInputModel : ListViewModelBase
    {
        public QuantityByInterviewersReportInputModel()
        {
            this.InterviewStatus = InterviewExportedAction.Completed;
        }

        public Guid SupervisorId { get; set; }
        public DateTime From { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Period { get; set; }
        public int ColumnCount { get; set; }
        public InterviewExportedAction InterviewStatus { get; set; }
    }
}