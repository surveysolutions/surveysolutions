using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class SingleQuestion : AbstractQuestion
    {
        public SingleQuestion()
        {
            this.Children = new List<IComposite>();
        }

        public SingleQuestion(Guid qid, string text) : base(text)
        {
            this.PublicKey = qid;
            this.Children = new List<IComposite>();
        }

        public override void AddAnswer(IAnswer answer)
        {
            if (answer == null)
            {
                return;
            }

            if (this.Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
            {
                throw new DuplicateNameException("answer with current public key already exist");
            }

            this.Answers.Add(answer);
        }
        
        public override T Find<T>(Guid publicKey)
        {
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return Enumerable.Empty<T>();
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return null;
        }
    }
}