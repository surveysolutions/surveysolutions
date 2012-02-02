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

        public CompleteAnswerView()
        {
        }
        public CompleteAnswerView(Guid questionPublicKey, RavenQuestionnaire.Core.Entities.SubEntities.IAnswer doc):base(questionPublicKey, doc)
        {
            this.Selected = false;
        }
        public CompleteAnswerView(RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer answer)
            : this(answer.QuestionPublicKey, answer)
        {
            this.Selected = answer.Selected;
        }
    }
}
