using System;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands
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
        public Answer[] Answers { get; set; }

        public UserLight Executor { get; set; }

        public AddNewQuestionCommand(string text, string stataExportCaption, QuestionType type, string questionnaireId, Guid? groupPublicKey,
            string condition, AnswerView[] answers, UserLight executor)
        {
            this.QuestionText = text;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = type;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = groupPublicKey;
            this.ConditionExpression = condition;
            this.Answers = new Answer[0];
            if (answers != null)
                this.Answers =
                    answers.Select(
                        a =>
                        new Answer()
                            {
                                AnswerValue = a.AnswerValue,
                                AnswerText = a.AnswerText,
                                AnswerType = a.AnswerType,
                                Mandatory = a.Mandatory,
                                PublicKey = a.PublicKey
                            }).ToArray();

            Executor = executor;
        }
    }
}
