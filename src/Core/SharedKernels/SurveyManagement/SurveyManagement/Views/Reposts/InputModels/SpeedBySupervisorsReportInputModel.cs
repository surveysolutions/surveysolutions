using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class SpeedBySupervisorsReportInputModel : ListViewModelBase
    {
        public SpeedBySupervisorsReportInputModel()
        {
            this.InterviewStatus = InterviewStatus.Completed;
        }

        public DateTime From { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Period { get; set; }
        public int ColumnCount { get; set; }
        public InterviewStatus InterviewStatus { get; set; }
    }
}
