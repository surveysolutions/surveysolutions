using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        [JsonIgnore]
        public override object Answer
        {
            get
            {
                var answers = this.Children.Where(c => ((ICompleteAnswer) c).Selected).Select(c => c.PublicKey);
                if (answers.Any()) return answers.First();
                return null;
            }
            set
            {
                if(value==null)
                    return;
                Guid selecteAnswer = Guid.Parse(value.ToString());
                var answerObject = this.FirstOrDefault<ICompleteAnswer>(a => a.PublicKey == selecteAnswer);
                if(answerObject!=null)
                {
                    this.Children.ForEach(c => ((ICompleteAnswer)c).Selected = false);
                    answerObject.Add(answerObject, null);
                    this.AnswerDate = DateTime.Now;
                    OnAdded(new CompositeAddedEventArgs(this));
                    return;
                }
                throw new CompositeException("answer wasn't found");
            }
        }


        public override string GetAnswerString()
        {
            var answer = this.Find<ICompleteAnswer>(a => a.Selected).FirstOrDefault();
            if (answer == null)
                return string.Empty;
            else return answer.AnswerText;
        }

        public override object GetAnswerObject()
        {
            return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).FirstOrDefault();
        }

        public override List<IComposite> Children { get; set; }

        public string AddSingleAttr { get; set; }

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
                        child.Add(c,null);
                        this.Children.ForEach(q => ((ICompleteAnswer) q).Selected = q.PublicKey == child.PublicKey);
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
