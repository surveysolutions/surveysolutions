namespace Main.Core.Entities.SubEntities.Question
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The text question.
    /// </summary>
    public class TextQuestion : AbstractQuestion, ITextCompleteQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public TextQuestion(string text)
            : base(text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextQuestion"/> class.
        /// </summary>
        public TextQuestion()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add text attr.
        /// </summary>
        public string AddTextAttr { get; set; }
        
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add answer.
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

        #endregion
    }
}