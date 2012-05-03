using System;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    public class AddNewQuestionCommand : ICommand
    {
        public string QuestionText
        {
            get;
            private set;
        }
        public string StataExportCaption
        {
            get;
            private set;
        }

        public string Instructions
        {
            get;
            private set;
        }

        public QuestionType QuestionType
        {
            get;
            private set;
        }
        public string QuestionnaireId
        {
            get;
            private set;
        }
        public Guid? GroupPublicKey
        {
            get;
            private set;
        }
        public string ConditionExpression
        {
            get;
            private set;
        }
        public string ValidationExpression
        {
            get;
            private set;
        }
        public bool Featured
        {
            get;
            private set;
        }
        public Order AnswerOrder
        {
            get;
            private set;
        }
        public Answer[] Answers { get; set; }

        public UserLight Executor { get; set; }

        public AddNewQuestionCommand(string text, string stataExportCaption, QuestionType type, string questionnaireId,
            Guid? groupPublicKey, string condition, string validation, string instructions, bool featured, Order answerOrder, AnswerView[] answers, UserLight executor)
        {
            QuestionText = text;
this.AnswerOrder = answerOrder;
            StataExportCaption = stataExportCaption;
            QuestionType = type;
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            GroupPublicKey = groupPublicKey;
            ConditionExpression = condition;
            Instructions = instructions;
            Answers = new Answer[0];
            this.ValidationExpression = validation;
            this.Featured = featured;
            if (answers != null)
                Answers = answers.Select(a => ConvertAnswer(a)).ToArray();

            Executor = executor;
        }

        private static Answer ConvertAnswer(AnswerView a)
        {
            var answer =  new Answer();
            answer.AnswerValue = a.AnswerValue;

            answer.AnswerType = a.AnswerType;
            answer.AnswerText = a.AnswerText;
            answer.Mandatory = a.Mandatory;
            answer.PublicKey = a.PublicKey;
            return answer;
        }
    }
}
