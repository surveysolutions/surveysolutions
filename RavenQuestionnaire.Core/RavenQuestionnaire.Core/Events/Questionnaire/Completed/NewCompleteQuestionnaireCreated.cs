using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Events
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewCompleteQuestionnaireCreated")]
    public class NewCompleteQuestionnaireCreated
    {
        public Guid CompletedQuestionnaireId { get; set; }
     
        public string QuestionnaireId { get; set; }

        public CompleteQuestionnaireDocument Questionnaire { set; get; }

        public DateTime CreationDate { get; set; }
        
    }
}
