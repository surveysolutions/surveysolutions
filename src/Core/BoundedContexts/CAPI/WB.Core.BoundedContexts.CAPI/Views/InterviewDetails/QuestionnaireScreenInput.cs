using System;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class QuestionnaireScreenInput
    {
        public QuestionnaireScreenInput(Guid questionnaireId)
        {
            QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}
