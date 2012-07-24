using System;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    public class UpdateQuestionCommand : ICommand
    {
        #region Properties

        public string QuestionnaireId { get; set; }
        public Guid QuestionPublicKey { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string ConditionExpression { get; private set; }
        public string ValidationExpression { get; private set; }
        public string StataExportCaption { get; private set; }
        public string Instructions { get; private set; }
        public Order AnswerOrder { get; private set; }
        public bool Featured { get; private set; }
        public bool Mandatory { get; private set; }
        public UserLight Executor { get; set; }
        public Answer[] Answers { get; set; }

        #endregion

        #region Constructor

        [JsonConstructor]
        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string questionText,
          string stataExportCaption, QuestionType questionType, string conditionExpression,
           string validationExpression, bool featured, bool mandatory, string instructions, Answer[] answers, Order answerOrder, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.AnswerOrder = answerOrder;
            this.QuestionPublicKey = questionPublicKey;
            this.QuestionText = questionText;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.Answers = new Answer[0];
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.Executor = executor;
            this.Featured = featured;
            this.Mandatory = mandatory;
            this.Instructions = instructions;
            if (answers != null)
                this.Answers = answers;
            else
            {
                this.Answers = new Answer[0];
            }
        }

        #endregion
    }
}
