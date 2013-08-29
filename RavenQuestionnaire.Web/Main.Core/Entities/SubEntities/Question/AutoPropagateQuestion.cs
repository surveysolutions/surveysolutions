namespace Main.Core.Entities.SubEntities.Question
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The auto propagate question.
    /// </summary>
    public class AutoPropagateQuestion : AbstractQuestion, IAutoPropagateQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateQuestion"/> class.
        /// </summary>
        public AutoPropagateQuestion()
        {
            this.Triggers = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public AutoPropagateQuestion(string text)
            : base(text)
        {
            this.Triggers = new List<Guid>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add numeric attr.
        /// </summary>
        public string AddNumericAttr { get; set; }

       /// <summary>
        /// Gets or sets the int attr.
        /// </summary>
        public int IntAttr { get; set; }

        #endregion

        #region Implementation of ITriggerable

        /// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        #endregion

        #region Implementation of IAutoPropagate

        /// <summary>
        /// Gets or sets MaxValue.
        /// </summary>
        public int MaxValue { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void AddAnswer(IAnswer answer)
        {
            throw new NotImplementedException();
        }
        

        /*/// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException();
        }*/

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
            return null;
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        public override IComposite Clone()
        {
            var question = base.Clone() as AutoPropagateQuestion;

            if (this.Triggers != null)
            {
                question.Triggers = new List<Guid>(this.Triggers);
            }

            return question;
        }

        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public override void Remove(IComposite c)
        {
            throw new CompositeException();
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
            throw new CompositeException();
        }*/

        #endregion 
    }
}