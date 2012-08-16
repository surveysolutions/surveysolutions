using System;

namespace RavenQuestionnaire.Core.Views.Assign
{
    public class AssignSurveyInputModel
    {
        public AssignSurveyInputModel(Guid id)
        {
            CompleteQuestionnaireId = id;
        }

        public Guid CompleteQuestionnaireId { get; private set; }
    }
}
