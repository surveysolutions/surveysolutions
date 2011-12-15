using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Answer
{
    public class AnswerView
    {
        public int Index { get; set; }
        public Guid PublicKey { get; set; }
        public string AnswerText { get; set; }
        public bool Mandatory { get; set; }
        public AnswerType AnswerType { get; set; }
        public Guid QuestionId { get; set; }

        private string _id;
        public  AnswerView()
        {
        }

        public  AnswerView(Guid publicKey, string text, bool mandatory, AnswerType type, Guid question)
        {
            this.PublicKey = publicKey;
            this.AnswerText = text;
            this.Mandatory = mandatory;
            this.AnswerType = type;
            this.QuestionId = question;
        }
        public static AnswerView New(Guid questionId)
        {
            return new AnswerView() { QuestionId = questionId };
        }
    }
}
