using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{

    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireAssignmentChanged")]
    public class QuestionnaireAssignmentChanged
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public UserLight Responsible { get; set; }
    }
}
