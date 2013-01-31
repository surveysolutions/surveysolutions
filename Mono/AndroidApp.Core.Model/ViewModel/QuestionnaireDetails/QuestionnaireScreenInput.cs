using System;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
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