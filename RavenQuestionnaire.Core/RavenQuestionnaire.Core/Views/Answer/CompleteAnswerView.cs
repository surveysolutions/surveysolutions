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

        public CompleteAnswerView(RavenQuestionnaire.Core.Entities.SubEntities.Answer answer,Guid questionPublicKey, bool selected)
            : base(questionPublicKey,answer)
        {
            this.Selected = selected;
        }

        public CompleteAnswerView(RavenQuestionnaire.Core.Entities.SubEntities.Answer answer, Guid questionPublicKey, bool selected, string customText)
            : this(answer,questionPublicKey, selected)
        {
            this.CustomAnswer = customText;
        }
    }
}
