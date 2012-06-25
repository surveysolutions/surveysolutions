using System;
using System.Linq;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    public class AddNewQuestionCommand : ICommand
    {
        #region Properties

        public string QuestionText { get; private set; }
        public string StataExportCaption { get; private set; }
        public string Instructions { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public string QuestionnaireId { get; private set; }
        public Guid? GroupPublicKey { get; private set; }
        public string ConditionExpression { get;private set; }
        public string ValidationExpression { get; private set; }
        public bool Featured { get; private set; }
        public Order AnswerOrder { get; private set; }

        public Guid PublicKey
        {
            get;
            private set;
        }

        public Answer[] Answers { get; set; }
        public UserLight Executor { get; set; }

        #endregion
        # region Constructor
        public AddNewQuestionCommand(string questionText, string stataExportCaption, QuestionType questionType, string questionnaireId,
            Guid? groupPublicKey, Guid publicKey, string conditionExpression, string validationExpression, string instructions, 
            bool featured, Order answerOrder, Answer[] answers, UserLight executor)
        {
            this.QuestionText = questionText;
            this.AnswerOrder = answerOrder;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = groupPublicKey;
            PublicKey = publicKey;
            this.Instructions = instructions;
            this.Answers = new Answer[0];
            this.ValidationExpression = validationExpression;
            ConditionExpression = conditionExpression;
            this.Featured = featured;
            Answers = answers;
            Executor = executor;
        }
        #endregion

        #region PrivateMetod


        #endregion
    }
}
