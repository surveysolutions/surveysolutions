// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeCompleteQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The date time complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The date time complete question.
    /// </summary>
    public sealed class DateTimeCompleteQuestion : AbstractCompleteQuestion, IDateTimeQuestion, ICompelteValueQuestion<DateTime?>
    {
        #region Fields


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public DateTimeCompleteQuestion(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeCompleteQuestion"/> class.
        /// </summary>
        public DateTimeCompleteQuestion()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add date time attr.
        /// </summary>
        public string AddDateTimeAttr { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public override List<IComposite> Children
        {
            get
            {
                return new List<IComposite>();
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the date time attr.
        /// </summary>
        public DateTime DateTimeAttr { get; set; }

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
            if (question == null || question.PublicKey != this.PublicKey)
                throw new CompositeException();
            this.Answer = question.Answer;
            this.AnswerDate = DateTime.Now;*/
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
                return new T[0];
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
            return this.Find(condition).FirstOrDefault();
        }

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public override object GetAnswerObject()
        {
            return this.Answer;
        }

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string GetAnswerString()
        {
            return this.Answer.HasValue
                       ? Convert.ToString(this.Answer.Value, CultureInfo.InvariantCulture)
                       : string.Empty;
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
            if (publicKey != this.PublicKey)
            {
                throw new CompositeException();
            }

            this.Answer = null;
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
        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            this.Answer = Convert.ToDateTime(answerValue, CultureInfo.InvariantCulture);
        }

        #endregion

        #region Implementation of ICompelteValueQuestion<DateTime>

        public DateTime? Answer { get; set; }

        #endregion
    }
}