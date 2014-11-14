using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class RecordFirstAnswerMarkerView : IView
    {
        public RecordFirstAnswerMarkerView(Guid interviewId)
        {
            this.InterviewId = interviewId;
        }

        public Guid InterviewId { get; private set; }
    }
}
