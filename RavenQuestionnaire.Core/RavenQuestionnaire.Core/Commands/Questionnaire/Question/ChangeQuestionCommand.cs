using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "ChangeQuestion")]
    public class ChangeQuestionCommand : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public Guid PublicKey { get; set; }
        public string QuestionText { get; set;}
        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public QuestionType QuestionType
        {
            get;
            set;
        }
        public Guid? GroupPublicKey
        {
            get;
            set;
        }
        public string ConditionExpression
        {
            get;
            set;
        }
        public string ValidationExpression
        {
            get;
            set;
        }
        public bool Featured
        {
            get;
            set;
        }
        public Order AnswerOrder
        {
            get;
            set;
        }
        public Answer[] Answers { get; set; }

        public ChangeQuestionCommand(Guid questionnaireId, Guid publicKey, string questionText, string stataExportCaption, QuestionType questionType,
                                                     string conditionExpression, string validationExpression, 
                                                     string instructions, bool featured, Order answerOrder, Answer[] answers)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            Instructions = instructions;
            this.Featured = featured;
            this.AnswerOrder = answerOrder;
            this.PublicKey = publicKey;
            this.Answers = answers;
        }
    }
}

