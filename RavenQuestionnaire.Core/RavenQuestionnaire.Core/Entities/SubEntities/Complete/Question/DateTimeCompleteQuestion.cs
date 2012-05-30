using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class DateTimeCompleteQuestion:AbstractCompleteQuestion, IDateTimeQuestion
    {

        #region Properties

        public override object Answer
        {
            get { return ((CompleteAnswer)(this.Children).FirstOrDefault()).AnswerValue; }
        }

        private object _answer;
        
        public override List<IComposite> Children { get; set; }

        public string AddDateTimeAttr { get; set; }

        public DateTime DateTimeAttr { get; set; }

        #endregion

        #region Method

        public override void Add(IComposite c, Guid? parent)
        {
            var question = c as ICompleteQuestion;
            if (question == null || question.PublicKey != this.PublicKey)
                throw new CompositeException();
            _answer = Convert.ToDateTime(question.Answer);
            this.AnswerDate = DateTime.Now;
            OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), c));
        }

        public override void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
        }

        public override void Remove(Guid publicKey)
        {
            if (publicKey != this.PublicKey)
                throw new CompositeException();
            OnRemoved(new CompositeRemovedEventArgs(this));
        }

        public override T Find<T>(Guid publicKey)
        {
            if (typeof(T).IsAssignableFrom(GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(CompleteAnswer)))
            {
                return (T) this.Children.SingleOrDefault();
            }
            return null;
        }


        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            if (!(this is T))
                return null;
            if (condition(this as T))
                return new T[] { this as T };
            return null;
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return Find<T>(condition).FirstOrDefault();
        }

        #endregion
    }
}
