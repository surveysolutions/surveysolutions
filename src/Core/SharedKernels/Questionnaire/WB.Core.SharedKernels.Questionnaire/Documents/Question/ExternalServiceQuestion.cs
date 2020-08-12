using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Core.Entities.SubEntities.Question
{
    public abstract class ExternalServiceQuestion : AbstractQuestion
    {
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
            return Enumerable.Empty<T>();
        }

        public override T? FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return null;
        }
    }
}
