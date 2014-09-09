using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class TakeNewInterviewInputModel
    {
        public TakeNewInterviewInputModel(Guid questionnaireId, long? version, Guid viewerId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = version;
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }

        public Guid QuestionnaireId { get; private set; }
        public long? QuestionnaireVersion { get; private set; }
    }
}