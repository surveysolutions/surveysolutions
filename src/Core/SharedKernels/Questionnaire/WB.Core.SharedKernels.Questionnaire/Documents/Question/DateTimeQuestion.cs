using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class DateTimeQuestion : AbstractQuestion
    {
        public DateTimeQuestion(string? questionText = null, List<IComposite>? children = null):base(questionText, children){ }

        public override QuestionType QuestionType => QuestionType.DateTime;

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
            return Array.Empty<T>();
        }

        public override T? FirstOrDefault<T>(Func<T, bool> condition) where T: class
        {
            return null;
        }
    }
}
