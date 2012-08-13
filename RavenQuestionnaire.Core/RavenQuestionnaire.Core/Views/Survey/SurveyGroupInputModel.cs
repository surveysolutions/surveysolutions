using System;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupInputModel
    {
        public string Id { get; set; }

        public string QuestionnaireId { get; set; }

        public SurveyGroupInputModel(string id)
        {
            this.Id = id;
        }

        public SurveyGroupInputModel(string id, string questionnaireId)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
        }
    }
}
