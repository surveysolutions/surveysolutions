using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class SpeedByInterviewersReportInputModel: ListViewModelBase
    {
        public SpeedByInterviewersReportInputModel()
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
