using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Question
{
    public class NumericQuestion:AbstractQuestion, INumericQuestion
    {
        public NumericQuestion()
        {
        }

        public NumericQuestion(string text) : base(text)
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
    }
}
