using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public sealed class AutoPropagateCompleteQuestion:AbstractCompleteQuestion, IAutoPropagate
    {

        public AutoPropagateCompleteQuestion()
        {
        }

        public AutoPropagateCompleteQuestion(string text)
            : base(text)
        {
        }
        public AutoPropagateCompleteQuestion(IAutoPropagate template)
        {
            this.TargetGroupKey = template.TargetGroupKey;
        }
       
        private int? _answer;

        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            _answer = Convert.ToInt32(answerValue);
        }

        public override string GetAnswerString()
        {
            return _answer.HasValue ? _answer.Value.ToString() : string.Empty;
        }

        public override object GetAnswerObject()
        {
            return _answer;
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

        #region Method

        public override void Add(IComposite c, Guid? parent)
        {
            throw new NotImplementedException();

            /*var question = c as ICompleteQuestion;
            if (question == null || question.PublicKey != this.PublicKey)
                throw new CompositeException();
            this.Answer = question.Answer;
            this.AnswerDate = DateTime.Now;*/
        }

        public override void Remove(IComposite c)
        {
           this.Remove(c.PublicKey);
        }

        public override void Remove(Guid publicKey)
        {
            if (publicKey != this.PublicKey)
                throw new CompositeException();
            this._answer = null;
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

        public Guid TargetGroupKey { get; set; }
    }
}
