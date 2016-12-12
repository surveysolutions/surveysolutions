using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    /// <summary>
    /// Single option question
    /// </summary>
    public class SingleQuestion : AbstractQuestion, ICategoricalQuestion
    {
        public SingleQuestion(string questionText = null, List<IComposite> children = null):base(questionText, children){ }

        public override void AddAnswer(Answer answer)
        {
            if (answer == null)
            {
                return;
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