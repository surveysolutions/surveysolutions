using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
       
        public AddNewQuestionCommand(string text, QuestionType type, string questionnaireId, Guid? groupPublicKey, string condition, AnswerView[] answers)
        {
            this.QuestionText = text;
            this.QuestionType = type;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = groupPublicKey;
            this.ConditionExpression = condition;
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
