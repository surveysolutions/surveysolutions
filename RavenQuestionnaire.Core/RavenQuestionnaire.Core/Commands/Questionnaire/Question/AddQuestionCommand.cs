using System;
using Ncqrs.Commanding;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "AddQuestion")]
    public class AddQuestionCommand : CommandBase
    {
        #region Properties
        
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set;}
        public string StataExportCaption { get; set; }
        public string Instructions { get; set; }
        public QuestionType QuestionType { get; set; }
        public Guid? GroupPublicKey { get; set; }
        public string ConditionExpression { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }
        public bool Featured { get; set;}
        public bool Mandatory { get;  set;}
        public Order AnswerOrder { get; set; }
        public Guid TargetGroupKey { get; set; }
        public Answer[] Answers { get; set; }

        #endregion

        public AddQuestionCommand(Guid questionnaireId, Guid publicKey, string questionText, string stataExportCaption, QuestionType questionType,
                                                        Guid? groupPublicKey,
                                                     string conditionExpression, string validationExpression, string validationMessage, string instructions,
                                                     bool featured, bool mandatory, Order answerOrder, 
                                                     Answer[] answers)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Featured = featured;
            this.Mandatory = mandatory;
            this.AnswerOrder = answerOrder;
            this.GroupPublicKey = groupPublicKey;
            this.Answers = answers;
            this.PublicKey = publicKey;
        }
        public AddQuestionCommand(Guid questionnaireId, Guid publicKey, string questionText, Guid TargetGroupKey, string stataExportCaption, QuestionType questionType,
                                                        Guid? groupPublicKey,
                                                     string conditionExpression, string validationExpression, string validationMessage, string instructions,
                                                     bool featured, bool mandatory, Order answerOrder,
                                                     Answer[] answers)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.TargetGroupKey = TargetGroupKey;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Featured = featured;
            this.Mandatory = mandatory;
            this.AnswerOrder = answerOrder;
            this.GroupPublicKey = groupPublicKey;
            this.Answers = answers;
            this.PublicKey = publicKey;
        }
    }
}

