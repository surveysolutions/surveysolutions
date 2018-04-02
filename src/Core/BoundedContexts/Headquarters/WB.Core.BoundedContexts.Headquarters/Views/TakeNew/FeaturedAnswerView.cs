using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.TakeNew
{
    public class FeaturedAnswerView 
    {
        public FeaturedAnswerView()
        {
        }

        public FeaturedAnswerView(Answer answer)
        {
            this.Id = Guid.NewGuid();
            this.Title = answer.AnswerText;
            this.AnswerValue = answer.AnswerValue;
        }

        public string AnswerValue { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }
    }
}