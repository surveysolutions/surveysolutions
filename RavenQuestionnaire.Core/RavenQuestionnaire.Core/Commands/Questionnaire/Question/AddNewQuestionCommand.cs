using System;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    public class AddNewQuestionCommand : ICommand
    {
        #region Properties
        public Guid qid { get; private set; }
        public string QuestionText { get; private set; }
        public string StataExportCaption { get; private set; }
        public string Instructions { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public string QuestionnaireId { get; private set; }
        public Guid? GroupPublicKey { get; private set; }
        public string ConditionExpression { get;private set; }
        public string ValidationExpression { get; private set; }
        public bool Featured { get; private set; }
        public bool Mandatory { get; private set; }
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
        public AddNewQuestionCommand(Guid qid, string questionText, string stataExportCaption, QuestionType questionType, string questionnaireId,
            Guid? groupPublicKey, Guid publicKey, string conditionExpression, string validationExpression, string instructions, 
            bool featured, bool mandatory, Order answerOrder, Answer[] answers, UserLight executor)
        {
            this.qid = qid;
            this.QuestionText = questionText;
            this.AnswerOrder = answerOrder;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = groupPublicKey;
            this.PublicKey = publicKey;
            this.Instructions = instructions;
            this.Answers = new Answer[0];
            this.ValidationExpression = validationExpression;
            this.ConditionExpression = conditionExpression;
            this.Featured = featured;
            this.Mandatory = mandatory;
            this.Answers = answers;
            this.Executor = executor;
        }
        #endregion

        #region PrivateMetod


        #endregion
    }
}
