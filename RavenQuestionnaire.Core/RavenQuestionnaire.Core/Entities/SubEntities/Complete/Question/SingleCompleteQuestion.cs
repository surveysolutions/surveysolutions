using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public sealed class SingleCompleteQuestion:AbstractCompleteQuestion, ISingleQuestion
    {
        #region Properties

        public SingleCompleteQuestion()
        {
            this.Children = new List<IComposite>();
        }

        public SingleCompleteQuestion(string text) : base(text)
        {
            this.Children = new List<IComposite>();
        }

        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            if (answer == null)
                return;
            
            Guid selecteAnswer = answer.First();

            var answerObject = this.FirstOrDefault<ICompleteAnswer>(a => a.PublicKey == selecteAnswer);
            if (answerObject != null)
            {
                this.Children.ForEach(c => ((ICompleteAnswer)c).Selected = false);
                answerObject.Add(answerObject, null);
                //this.AnswerDate = DateTime.Now;
                return;
            }
            throw new CompositeException("answer wasn't found");
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
            /*return (
                this.Children.Where(c => ((ICompleteAnswer)c).Selected))
                .Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).FirstOrDefault();

*/
            var answers = this.Children.Where(c => ((ICompleteAnswer)c).Selected).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText);
            if (answers.Any())
                return answers.First();
            return null;

        }

        public override List<IComposite> Children { get; set; }

        public string AddSingleAttr { get; set; }

        #endregion

        #region Method

        public override void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException();
            /*var question = c as ICompleteQuestion;
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
            throw new CompositeException();*/
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
                
                return;
            }
            foreach (CompleteAnswer completeAnswer in this.Children)
            {
                try
                {
                    completeAnswer.Remove(publicKey);
                  
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
