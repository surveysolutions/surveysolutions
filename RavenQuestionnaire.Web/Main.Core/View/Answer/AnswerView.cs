using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.Answer
{
    public class AnswerView : ICompositeView
    {
        public AnswerView()
        {
        }

        public AnswerView(Guid questionPublicKey, IAnswer answer)
        {
            this.Id = answer.PublicKey;
            this.Title = answer.AnswerText;
            this.AnswerValue = answer.AnswerValue;
        }

        public AnswerView(Guid questionPublicKey)
        {
        }

    
        public string AnswerValue { get; set; }

        
        public Guid Id { get; set; }

        public string Title { get; set; }
    }
}