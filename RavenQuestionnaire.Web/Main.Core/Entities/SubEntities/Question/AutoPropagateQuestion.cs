using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class AutoPropagateQuestion : AbstractQuestion, IAutoPropagateQuestion
    {
        public AutoPropagateQuestion()
        {
            this.Triggers = new List<Guid>();
        }

        public AutoPropagateQuestion(string text)
            : base(text)
        {
            this.Triggers = new List<Guid>();
        }

        public List<Guid> Triggers { get; set; }
       
        public int MaxValue { get; set; }

        public override void AddAnswer(IAnswer answer)
        {
            throw new NotImplementedException();
        }

        public override T Find<T>(Guid publicKey)
        {
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return new T[0];
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return null;
        }

        public override IComposite Clone()
        {
            var question = base.Clone() as AutoPropagateQuestion;

            if (this.Triggers != null)
            {
                question.Triggers = new List<Guid>(this.Triggers);
            }

            return question;
        }
    }
}