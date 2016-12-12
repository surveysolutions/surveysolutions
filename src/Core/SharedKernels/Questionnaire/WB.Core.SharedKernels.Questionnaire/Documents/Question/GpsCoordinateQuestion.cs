using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class GpsCoordinateQuestion : AbstractQuestion
    {
        public GpsCoordinateQuestion(string questionText = null, List<IComposite> children = null):base(questionText, children){ }

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