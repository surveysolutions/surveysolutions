using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    public class NumericQuestion : AbstractQuestion, INumericQuestion
    {
        public NumericQuestion(string? questionText = null, List<IComposite>? children = null):base(questionText, children){ }

        public override QuestionType QuestionType => QuestionType.Numeric;

        public override void AddAnswer(Answer answer)
        {
            if (answer == null)
            {
                return;
            }

            this.Answers.Add(answer);
        }

        public override T? Find<T>(Guid publicKey) where T:class
        {
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return Array.Empty<T>();
        }

        public override T? FirstOrDefault<T>(Func<T, bool> condition) where T: class
        {
            return null;
        }

        public bool IsInteger { get; set; }
        
        public int? CountOfDecimalPlaces { get; set; }

        public bool UseFormatting { get; set; }
    }
}
