// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPropagateCompleteQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The auto propagate complete question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The auto propagate complete question.
    /// </summary>
    public sealed class AutoPropagateCompleteQuestion : AbstractCompleteQuestion, IAutoPropagate, ICompelteValueQuestion<int?>
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateCompleteQuestion"/> class.
        /// </summary>
        public AutoPropagateCompleteQuestion()
        {
            this.Triggers = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateCompleteQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public AutoPropagateCompleteQuestion(string text)
            : base(text)
        {
            this.Triggers = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateCompleteQuestion"/> class.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        public AutoPropagateCompleteQuestion(IAutoPropagate template)
        {
            this.Triggers = template.Triggers;
        }

        #endregion

        #region Public Properties

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
        /// Gets or sets the target group key.
        /// </summary>
        public Guid TargetGroupKey { get; set; }

        #endregion

        // {
        // get { return new List<IComposite>(); }
        // set { }
        // }
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
            return this.Answer.HasValue ? this.Answer.Value.ToString() : string.Empty;
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
            this.Answer = Convert.ToInt32(answerValue);
        }

        #endregion

        #region Implementation of ICompelteValueQuestion<int>

        public int? Answer { get; set; }

        #endregion

        #region Implementation of ITriggerable

        public List<Guid> Triggers { get; set; }

        #endregion
    }
}