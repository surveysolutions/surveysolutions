using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.Questionnaire
{
    public class FullQuestionDataEvent
    {
        public Order AnswerOrder { get; set; }
        public Answer[] Answers { get; set; }
        public string ConditionExpression { get; set; }
        public bool Featured { get; set; }
        public Guid? GroupPublicKey { get; set; }
        public string Instructions { get; set; }
        public bool Mandatory { get; set; }
        public bool Capital { get; set; }
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public QuestionScope QuestionScope { get; set; }
        public string StataExportCaption { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }
        public List<Guid> Triggers { get; set; }
        public int MaxValue { get; set; }
    }
}