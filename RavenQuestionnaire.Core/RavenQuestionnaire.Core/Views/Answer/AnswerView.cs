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

        public  AnswerView()
        {
        }

        public AnswerView(Guid questionPublicKey, RavenQuestionnaire.Core.Entities.SubEntities.Answer doc)
        {
            this.PublicKey = doc.PublicKey;
            this.AnswerText = doc.AnswerText;
            this.Mandatory = doc.Mandatory;
            this.AnswerType = doc.AnswerType;
            this.QuestionId = questionPublicKey;
        }

        public  AnswerView (Guid questionPublicKey)
        {
            this.QuestionId = questionPublicKey;
        }
    }
}
