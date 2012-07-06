using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{

    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireStatusChanged")]
    public class QuestionnaireStatusChanged
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public SurveyStatus Status { get; set; }
    }
}
