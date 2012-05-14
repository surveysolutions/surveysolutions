using System;
namespace RavenQuestionnaire.Core.Views.Answer
{
    public class CompleteAnswerView : AnswerView
    {
        public bool Selected { get; set; }
        
        public CompleteAnswerView()
        {
        }
        public CompleteAnswerView(Guid questionPublicKey, Entities.SubEntities.IAnswer doc):base(questionPublicKey, doc)
        {
            this.Selected = false;
        }
        public CompleteAnswerView(Entities.SubEntities.Complete.ICompleteAnswer answer)
            : this(answer.QuestionPublicKey, answer)
        {
            this.Selected = answer.Selected;
        }
    }
}
