using System;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireViewInputModel
    {
        public QuestionnaireViewInputModel(Guid id)
        {
            QuestionnaireId = id;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}
