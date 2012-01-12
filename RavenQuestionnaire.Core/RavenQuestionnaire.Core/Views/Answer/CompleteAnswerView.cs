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

        public CompleteAnswerView(RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer answer,Guid questionPublicKey)
            : base(questionPublicKey,answer)
        {
            this.Selected = answer.Selected;
            this.CustomAnswer = answer.CustomAnswer;
        }
    }
}
