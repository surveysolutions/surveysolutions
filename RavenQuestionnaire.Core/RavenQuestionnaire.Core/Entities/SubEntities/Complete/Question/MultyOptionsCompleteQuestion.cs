using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public sealed class MultyOptionsCompleteQuestion:AbstractCompleteQuestion, IMultyOptionsQuestion
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
        [JsonIgnore]
        public override object Answer
        {
            get
            {
                var answers = CollectAnswers();
                return !answers.Any() ? null : answers;
            }
            set
            {
                
                var selecteAnswers = value as IEnumerable<Guid>;
                var answerObjects =
                    this.Find<ICompleteAnswer>(
                        a => selecteAnswers.Count(q => q.ToString() == a.PublicKey.ToString()) > 0);
                if (answerObjects != null)
                {
                    this.Children.ForEach(c => ((ICompleteAnswer)c).Selected = false);
                    foreach (ICompleteAnswer completeAnswer in answerObjects)
                    {
                        completeAnswer.Add(completeAnswer, null);
                    }
                    this.AnswerDate = DateTime.Now;
                    return;
                }
                throw new CompositeException("answer wasn't found");
            }
        }

        private IEnumerable<Guid> CollectAnswers()
        {
           //  return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).FirstOrDefault(); 
            return this.Children.Where(c => ((ICompleteAnswer)c).Selected).Select(c => c.PublicKey);

        }


        public override string GetAnswerString()
        {
            var answers = this.Find<ICompleteAnswer>(a => a.Selected);
            if (!answers.Any())
                return string.Empty;
            else return string.Join(", ", answers.Select(a => a.AnswerText));
        }

        public override object GetAnswerObject()
        {
            return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText);
        }

        public override List<IComposite> Children { get; set; }

        public string AddMultyAttr { get; set; }

        #endregion

        #region Method

        public override void Add(IComposite c, Guid? parent)
        {
            var question = c as ICompleteQuestion;
            if (question != null && question.PublicKey == this.PublicKey)
            {
                this.Answer = question.Answer;
                return;
            }
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer != null)
            {
                foreach (IComposite child in this.Children)
                {
                    try
                    {
                        child.Add(c, null);
                        return;
                    }
                    catch (CompositeException)
                    {

                    }
                }
                //this.Answer = currentAnswer.PublicKey;

            }
            throw new CompositeException();
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
                
                return;

            }
            foreach (ICompleteAnswer composite in Children)
            {
                try
                {

                    composite.Remove(publicKey);
                    
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
