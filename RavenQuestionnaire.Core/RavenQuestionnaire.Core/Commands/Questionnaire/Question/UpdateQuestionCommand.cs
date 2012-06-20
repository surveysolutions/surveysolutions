using System;
using System.Linq;
using Newtonsoft.Json;
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
        public bool Featured
        {
            get;
            private set;
        }
        public UserLight Executor { get; set; }

        public Answer[] Answers { get; set; }
        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string questionText,
          string stataExportCaption, QuestionType questionType, string conditionExpression,
           string validationExpression, bool featured, string instructions, Answer[] answers, Order answerOrder, UserLight executor)
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
            this.Instructions = instructions;
            this.Answers = answers;
        }
        [JsonConstructor]
        public UpdateQuestionCommand(string questionnaireId, Guid questionPublicKey, string questionText,
           string stataExportCaption, QuestionType questionType, string conditionExpression, 
            string validationExpression, bool featured, string instructions, AnswerView[] answers, Order answerOrder, UserLight executor):
            this(questionnaireId,questionPublicKey,questionText,stataExportCaption,questionType,conditionExpression,validationExpression,featured,instructions,new Answer[0],answerOrder,executor)
        {
            if (answers != null)
                this.Answers =
                    answers.Select(
                        a =>
                        new Answer()
                            {
                                AnswerValue = a.AnswerValue,
                                AnswerText = a.Title,
                                AnswerType = a.AnswerType,
                                Mandatory = a.Mandatory,
                                PublicKey = a.PublicKey,
                                AnswerImage =
                                    string.IsNullOrEmpty(a.AnswerImage) ? "" : IdUtil.CreateFileId(a.AnswerImage)
                            }).ToArray();
        }
    }
}
