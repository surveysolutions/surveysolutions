using System;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
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
        public string ValidationExpression
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

        public Order AnswerOrder
        {
            get;
            private set;
        }
        public UserLight Executor { get; set; }

        public Answer[] Answers { get; set; }
        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string text,
            string stataExport, QuestionType type, string condition, string validation, string instructions, Order answerOrder, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.AnswerOrder = answerOrder;
            this.QuestionPublicKey = questionPublicKey;
            this.QuestionText = text;
            this.StataExportCaption = stataExport;
            this.QuestionType = type;
            this.Answers = new Answer[0];
            this.ConditionExpression = condition;
            this.ValidationExpression = validation;
            this.Executor = executor;
            this.Instructions = instructions;
        }

        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string text,
           string stataExport, QuestionType type, string condition, string validation, string instructions, AnswerView[] answers, Order answerOrder, UserLight executor) :
            this(questionnaireId, questionPublicKey, text, stataExport, type, condition, validation, instructions, answerOrder, executor)
        {
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
                                PublicKey = a.PublicKey,
                                AnswerImage = string.IsNullOrEmpty(a.AnswerImage)? "": IdUtil.CreateFileId(a.AnswerImage)
                            }).ToArray();
        }
    }
}
