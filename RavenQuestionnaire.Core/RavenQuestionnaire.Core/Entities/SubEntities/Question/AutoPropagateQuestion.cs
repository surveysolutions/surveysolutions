using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Question
{
    public class AutoPropagateQuestion : AbstractQuestion, IAutoPropagate
    {
        public AutoPropagateQuestion()
        {
        }

        public AutoPropagateQuestion(string text)
            : base(text)
        {
        }

        public override void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException();
        }

        public override void Remove(IComposite c)
        {
            throw new CompositeException();
        }

        public override void Remove(Guid publicKey)
        {
            throw new CompositeException();
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

        public override List<IComposite> Children
        {
            get { return new List<IComposite>(0);}
            set { }
        }

        public string AddNumericAttr { get; set; }

        public int IntAttr { get; set; }

        public Guid TargetGroupKey { get; set; }
    }
}
