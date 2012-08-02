using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AnswerSet")]
    public class AnswerSet
    {
        public Guid CompletedQuestionnaireId { get; set; }//is it necessary?
        
        public Guid QuestionPublicKey { set; get; }
        public Guid? PropogationPublicKey { set; get; }

        public object Answer { set; get; }
        public String AnswerString { set; get; }

        public bool Featured { set; get; }

        public string QuestionText { get; set; }
    }
}
