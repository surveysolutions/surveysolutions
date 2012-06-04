using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class NumericCompleteQuestion:AbstractCompleteQuestion, INumericQuestion
    {
        #region Properties

        public NumericCompleteQuestion()
        {
        }

        public NumericCompleteQuestion(string text) : base(text)
        {
        }

        public override object Answer
        {
            get { return answer; }
            set
            {
                if (value != null)
                answer = Convert.ToInt32(value);
               
            }
        }
        private int? answer;

        public override string GetAnswerString()
        {
            return answer.HasValue ? answer.Value.ToString() : string.Empty;
        }

        public override List<IComposite> Children
        {
            get { return new List<IComposite>(); }
            set { }
        }
        //{
        //    get { return new List<IComposite>(); }
        //    set { }
        //}

        public string AddNumericAttr { get; set; }

        public int IntAttr { get; set; }

        #endregion

        #region Method

        public override void Add(IComposite c, Guid? parent)
        {
            var question = c as ICompleteQuestion;
            if (question == null || question.PublicKey != this.PublicKey)
                throw new CompositeException();
            this.Answer = question.Answer;
            this.AnswerDate = DateTime.Now;
            OnAdded(new CompositeAddedEventArgs(this));
        }

        public override void Remove(IComposite c)
        {
           this.Remove(c.PublicKey);
        }

        public override void Remove(Guid publicKey)
        {
            if (publicKey != this.PublicKey)
                throw new CompositeException();
            this.answer = null;
        }

        public override T Find<T>(Guid publicKey)
        {
            if (typeof(T).IsAssignableFrom(GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            if (!(this is T))
                return new T[0];
            if (condition(this as T))
                return new T[] { this as T };
            return new T[0];
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return Find<T>(condition).FirstOrDefault();
        }

        #endregion
    }
}
