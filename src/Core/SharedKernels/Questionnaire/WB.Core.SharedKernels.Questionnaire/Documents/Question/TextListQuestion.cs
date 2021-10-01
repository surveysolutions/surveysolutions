using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    public class TextListQuestion : AbstractQuestion, ITextListQuestion
    {
        public TextListQuestion(string? questionText = null, List<IComposite>? children = null):base(questionText, children){ }

        public override QuestionType QuestionType => QuestionType.TextList;

        public override void AddAnswer(Answer answer)
        {
            throw new NotImplementedException();
        }
    
        public override T? Find<T>(Guid publicKey) where T: class
        {
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return new T[0];
        }
     
        public override T? FirstOrDefault<T>(Func<T, bool> condition) where T: class
        {
            return null;
        }

        public int? MaxAnswerCount { get; set; }
        
        public static int MaxAnswerCountLimit => Constants.MaxLongRosterRowCount;
    }
}
