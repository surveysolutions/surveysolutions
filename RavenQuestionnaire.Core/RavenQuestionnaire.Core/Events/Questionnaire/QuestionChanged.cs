using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionChangeded")]
    public class QuestionChanged
    {
        public Guid PublicKey { set; get; }

        public string QuestionText;

        public string ConditionExpression { get; set; }

        public QuestionType QuestionType { get; set; }

        public string StataExportCaption { get; set; }

        public string ValidationExpression { get; set; }

        public Order AnswerOrder { get; set; }

        public bool Featured { get; set; }

        public bool Mandatory { get; set; }

        public Answer[] Answers { get; set; }

        public string Instructions { get; set; }
    }
}
