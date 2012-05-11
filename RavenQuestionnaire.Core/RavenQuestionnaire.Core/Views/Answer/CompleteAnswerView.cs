using System;
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
        public CompleteAnswerView(Guid questionKey, RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteAnswer answer)
            : base(questionKey, answer)
        {
            this.Selected = answer.Selected;
        }
    }
}
