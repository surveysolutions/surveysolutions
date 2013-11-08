using System;
using Main.Core.Entities.SubEntities;
using Main.Core.View;

namespace Core.Supervisor.Views.TakeNew
{
    public class FeaturedAnswerView : ICompositeView
    {
        public FeaturedAnswerView()
        {
        }

        public FeaturedAnswerView(IAnswer answer)
        {
            this.Id = answer.PublicKey;
            this.Title = answer.AnswerText;
            this.AnswerValue = answer.AnswerValue;
        }

        public string AnswerValue { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }
    }
}