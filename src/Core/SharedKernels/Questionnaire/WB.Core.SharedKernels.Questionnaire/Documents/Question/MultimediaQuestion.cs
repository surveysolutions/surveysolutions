using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Core.Entities.SubEntities.Question
{
    public class MultimediaQuestion : AbstractQuestion, IMultimediaQuestion
    {
        public MultimediaQuestion() {}

        public MultimediaQuestion(string text)
            : base(text) {}

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
