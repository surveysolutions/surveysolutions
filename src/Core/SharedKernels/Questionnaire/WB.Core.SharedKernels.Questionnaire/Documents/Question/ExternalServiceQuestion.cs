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