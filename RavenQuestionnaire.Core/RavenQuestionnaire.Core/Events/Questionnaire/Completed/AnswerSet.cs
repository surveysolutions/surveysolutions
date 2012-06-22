using System;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AnswerSet")]
    public class AnswerSet
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public Guid QuestionPublicKey { set; get; }
        public Guid? PropogationPublicKey { set; get; }

        public object Answer { set; get; }
    }
}
