using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Answer
{
    public class CompleteAnswerView : AnswerView
    {
        public bool Selected { get; set; }
        public string CustomAnswer { get; set; }

        public CompleteAnswerView()
        {
        }

        public CompleteAnswerView(AnswerView answer, bool selected)
            : base(answer.PublicKey, answer.AnswerText, answer.Mandatory, answer.AnswerType, answer.QuestionId)
        {
            this.Selected = selected;
        }

        public CompleteAnswerView(AnswerView answer, bool selected, string customText)
            : this(answer, selected)
        {
            this.CustomAnswer = customText;
        }
    }
}
