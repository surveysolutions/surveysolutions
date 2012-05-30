using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class SingleCompleteQuestion:AbstractCompleteQuestion, ISingleQuestion
    {
        #region Properties

        public SingleCompleteQuestion()
        {
            this.Children=new List<IComposite>();
        }

        public SingleCompleteQuestion(string text) : base(text)
        {
            this.Children = new List<IComposite>();
        }

        public override object Answer
        {
            get { return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).FirstOrDefault(); }
        }
      

        public override List<IComposite> Children { get; set; }

        public string AddSingleAttr { get; set; }

        #endregion

        #region Method

        public override void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || !this.Children.Any(a => a.PublicKey == currentAnswer.PublicKey))
                throw new CompositeException("answer wasn't found");
            foreach (CompleteAnswer completeAnswer in this.Children)
            {
                try
                {
                    completeAnswer.Add(c, parent);
                    foreach (IComposite answer in this.Children)
                        if (answer.PublicKey != currentAnswer.PublicKey) 
                            ((CompleteAnswer)answer).Selected = false;
                    this.AnswerDate = DateTime.Now;
                    OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), c));
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException("answer wasn't found");

        }

        public override void Remove(IComposite c)
        {
            Remove(c.PublicKey);
        }

        public override void Remove(Guid publicKey)
        {

            if (this.PublicKey == publicKey)
            {
                foreach (CompleteAnswer answer in this.Children)
                    answer.Remove(answer);
                OnRemoved(new CompositeRemovedEventArgs(this));
                return;
            }
            foreach (CompleteAnswer completeAnswer in this.Children)
            {
                try
                {
                    completeAnswer.Remove(publicKey);
                    OnRemoved(new CompositeRemovedEventArgs(new CompositeRemovedEventArgs(this), completeAnswer));
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException("answer wasn't found");


        }

        public override T Find<T>(Guid publicKey)
        {
            if (typeof(T).IsAssignableFrom(GetType()))
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            if (typeof(T).IsAssignableFrom(typeof(CompleteAnswer)))
                return this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            return
                 Children.Where(a => a is T && condition(a as T)).Select
                     (a => a as T);
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return Find<T>(condition).FirstOrDefault();
        }

        #endregion
    }
}
