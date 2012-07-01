using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Question
{
    public class SingleQuestion:AbstractQuestion, ISingleQuestion
    {
        public SingleQuestion()
            : base()
        {
            this.Children=new List<IComposite>();
        }
        public SingleQuestion(Guid qid, string text)
            : base(text)
        {
            this.PublicKey = qid;
            this.Children = new List<IComposite>();
        }
        public override void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                IAnswer answer = c as IAnswer;
                if (answer != null)
                {
                    // AddAnswer(answer);
                    if (Children.Any(a => a.PublicKey.Equals(answer.PublicKey)))
                        throw new DuplicateNameException("answer with current publick key already exist");
                    Children.Add(answer);
                    return;
                }
            }
            throw new CompositeException();
        }

        public override void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
        }

        public override void Remove(Guid publicKey)
        {
            if (Children.RemoveAll(a => a.PublicKey.Equals(publicKey)) > 0)
            {
                return;
            }

            throw new CompositeException();
        }

        public override T Find<T>(Guid publicKey)
        {
            if (typeof(T).IsAssignableFrom(typeof(IAnswer)))
                return Children.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return Children.Where(a => a is T && condition(a as T)).Select(a => a as T);
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return Children.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault();
        }

        public override List<IComposite> Children { get; set; }

        public string AddSingleAttr { get; set; }
    }
}
