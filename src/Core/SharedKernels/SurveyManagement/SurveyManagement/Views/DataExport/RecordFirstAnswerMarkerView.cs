using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class RecordFirstAnswerMarkerView : IView
    {
        public RecordFirstAnswerMarkerView(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public Guid InterviewId { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
    }
}
