using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    public class TextListQuestion : AbstractQuestion, ITextListQuestion
    {
        public TextListQuestion(string text)
            : base(text)
        {
        }

        public TextListQuestion()
        {
        }

        public override void AddAnswer(Answer answer)
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

        public int? MaxAnswerCount { get; set; }
        
        public static int MaxAnswerCountLimit {
            get { return Constants.MaxRosterRowCount; }
        }
    }
}