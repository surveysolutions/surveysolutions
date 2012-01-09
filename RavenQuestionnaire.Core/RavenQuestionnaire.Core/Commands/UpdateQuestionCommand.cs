using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateQuestionCommand : ICommand
    {
        public string QuestionnaireId { get; set; }
        public Guid QuestionPublicKey { get; set; }

        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string ConditionExpression
        {
            get;
            private set;
        }

        public string StataExportCaption
        {
            get;
            private set;
        }

        public UserLight Executor { get; set; }

        public Answer[] Answers { get; set; }
        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string text,
            string stataExport, QuestionType type, string condition, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.QuestionPublicKey = questionPublicKey;
            this.QuestionText = text;
            this.StataExportCaption = stataExport;
            this.QuestionType = type;
            this.Answers = new Answer[0];
            this.ConditionExpression = condition;
            this.Executor = executor;
        }

        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string text,
           string stataExport, QuestionType type, string condition, AnswerView[] answers, UserLight executor) :
            this(questionnaireId, questionPublicKey, text, stataExport, type, condition, executor)
        {
            this.Answers =
                answers.Select(
                    a =>
                    new Answer()
                        {
                            AnswerText = a.AnswerText,
                            AnswerType = a.AnswerType,
                            Mandatory = a.Mandatory,
                            PublicKey = a.PublicKey
                        }).ToArray();
        }
    }
}
