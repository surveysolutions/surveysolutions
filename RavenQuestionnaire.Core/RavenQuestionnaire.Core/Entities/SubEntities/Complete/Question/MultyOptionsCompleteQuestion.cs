using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class MultyOptionsCompleteQuestion:AbstractCompleteQuestion, IMultyOptionsQuestion
    {

        #region Properties

        public MultyOptionsCompleteQuestion()
        {
            this.Children = new List<IComposite>();
        }

        public MultyOptionsCompleteQuestion(string text) : base(text)
        {
            this.Children = new List<IComposite>();
        }

        public override object Answer
        {
            get { return CollectAnswers(); }
        }

        private IEnumerable<object> CollectAnswers()
        {
           //  return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).FirstOrDefault(); 
            return this.Children.Where(c => ((ICompleteAnswer)c).Selected).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText);

        }


        public override List<IComposite> Children { get; set; }

        public string AddMultyAttr { get; set; }

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
                    this.AnswerDate = DateTime.Now;
                     OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), c));
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException("answer wasn't found");


          /*  var question = c as ICompleteQuestion;
            if (question == null || question.PublicKey != this.PublicKey)
                throw new CompositeException();
            var answers = CollectAnswers();

            foreach (var answer in answers)
            {
                var completeAnswer = answer as CompleteAnswer;
                if (completeAnswer != null)
                {
                    completeAnswer.Add(c, parent);
                    return;
                }


                this.AnswerDate = DateTime.Now;
                OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), c));
            }*/
        }

        public override void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
        }

        public override void Remove(Guid publicKey)
        {
            if (publicKey == this.PublicKey)
            {
                foreach (ICompleteAnswer composite in Children)
                {
                    composite.Remove(composite.PublicKey);
                }
                OnRemoved(new CompositeRemovedEventArgs(this));
                return;

            }
            foreach (ICompleteAnswer composite in Children)
            {
                try
                {

                    composite.Remove(publicKey);
                    OnRemoved(new CompositeRemovedEventArgs(this));
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            throw new CompositeException();

        }

        public override T Find<T>(Guid publicKey)
        {
            if (typeof(T).IsAssignableFrom(GetType()))
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            if (typeof(T).IsAssignableFrom(typeof(CompleteAnswer)))
            {
                return this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            }
            return null;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            if (!(this is T))
                return
                    Children.Where(a => a is T && condition(a as T)).Select
                        (a => a as T);
            if (condition(this as T))
                return new T[] {this as T};
            return new T[0];
        }

        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return Find<T>(condition).FirstOrDefault();
        }

        #endregion
 
    }
}
