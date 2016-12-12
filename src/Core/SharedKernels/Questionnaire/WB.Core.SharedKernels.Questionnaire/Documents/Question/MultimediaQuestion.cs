using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class MultimediaQuestion : AbstractQuestion, IMultimediaQuestion
    {
        public MultimediaQuestion(string questionText = null, List<IComposite> children = null):base(questionText, children){ }

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

        public override QuestionType QuestionType
        {
            get { return QuestionType.Multimedia; }
            set { }
        }
    }
}
