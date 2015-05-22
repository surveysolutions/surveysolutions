using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class QuantityByInterviewersReportInputModel : ListViewModelBase
    {
        public QuantityByInterviewersReportInputModel()
        {
            this.InterviewStatus = InterviewStatus.Completed;
        }

        public Guid SupervisorId { get; set; }
        public DateTime From { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Period { get; set; }
        public int ColumnCount { get; set; }
        public InterviewStatus InterviewStatus { get; set; }
    }
}