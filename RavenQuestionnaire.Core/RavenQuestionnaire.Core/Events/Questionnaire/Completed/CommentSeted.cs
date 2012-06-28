using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AnswerSet")]
    public class CommentSeted
    {
        public Guid CompleteQuestionnaireId { get; set; }
        public Guid QuestionPublickey { get;  set; }
        public string Comments { get; set; }
        public Guid? PropogationPublicKey { get; set; }
    }
}
