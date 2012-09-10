// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultyOptionsCompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The multy options complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Entities.Composite;

    /// <summary>
    /// The multy options complete question.
    /// </summary>
    public sealed class MultyOptionsCompleteQuestion : AbstractCompleteQuestion, IMultyOptionsQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsCompleteQuestion"/> class.
        /// </summary>
        public MultyOptionsCompleteQuestion()
        {
            this.Children = new List<IComposite>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public MultyOptionsCompleteQuestion(string text)
            : base(text)
        {
            this.Children = new List<IComposite>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add multy attr.
        /// </summary>
        public string AddMultyAttr { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public override List<IComposite> Children { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
            throw new NotImplementedException();

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
                        child.Add(c, null);
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

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public override T Find<T>(Guid publicKey)
        {
            if (typeof(T).IsAssignableFrom(this.GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                {
                    return this as T;
                }
            }

            if (typeof(T).IsAssignableFrom(typeof(CompleteAnswer)))
            {
                return this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(
                    result => result != null);
            }

            return null;
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        public override IEnumerable<T> Find<T>(Func<T, bool> condition)
        {
            if (!(this is T))
            {
                return this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T);
            }

            if (condition(this as T))
            {
                return new[] { this as T };
            }

            return new T[0];
        }

        /// <summary>
        /// The first or default.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public override T FirstOrDefault<T>(Func<T, bool> condition)
        {
            return Find(condition).FirstOrDefault();
        }

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public override object GetAnswerObject()
        {
            // return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText);
            IEnumerable<Guid> answers = this.CollectAnswers();
            return !answers.Any() ? null : answers;
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetAnswerString()
        {
            IEnumerable<ICompleteAnswer> answers = this.Find<ICompleteAnswer>(a => a.Selected);
            if (!answers.Any())
            {
                return string.Empty;
            }
            else
            {
                return string.Join(", ", answers.Select(a => a.AnswerText));
            }
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        public override void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public override void Remove(Guid publicKey)
        {
            if (publicKey == this.PublicKey)
            {
                foreach (ICompleteAnswer composite in this.Children)
                {
                    composite.Remove(composite.PublicKey);
                }

                return;
            }

            foreach (ICompleteAnswer composite in this.Children)
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

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <param name="answerValue">
        /// The answer value.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        /// <exception cref="CompositeException">
        /// </exception>
        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            if (answer == null)
            {
                throw new Exception("Parameter: answer");
            }

            List<Guid> selecteAnswers = answer;
            IEnumerable<ICompleteAnswer> answerObjects =
                this.Find<ICompleteAnswer>(a => selecteAnswers.Count(q => q.ToString() == a.PublicKey.ToString()) > 0);
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

        #endregion

        #region Methods

        /// <summary>
        /// The collect answers.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Guid].
        /// </returns>
        private IEnumerable<Guid> CollectAnswers()
        {
            // return (this.Children.Where(c => ((ICompleteAnswer)c).Selected)).Select(c => ((ICompleteAnswer)c).AnswerValue ?? ((ICompleteAnswer)c).AnswerText).FirstOrDefault(); 
            return this.Children.Where(c => ((ICompleteAnswer)c).Selected).Select(c => c.PublicKey);
        }

        #endregion
    }
}