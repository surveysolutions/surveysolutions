using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Main.Core.Entities.SubEntities.Question
{
    public class MultyOptionsQuestion : AbstractQuestion, IMultyOptionsQuestion
    {
        public MultyOptionsQuestion()
        {
        }

        public MultyOptionsQuestion(string text)
            : base(text)
        {
        }

        public override void AddAnswer(IAnswer answer)
        {
            if (answer == null)
            {
                return;
            }

            if (this.Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
            {
                throw new DuplicateNameException("answer with current publick key already exist");
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

        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
    }
}