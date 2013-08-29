namespace Main.Core.Entities.SubEntities.Question
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The multy options question.
    /// </summary>
    public class MultyOptionsQuestion : AbstractQuestion, IMultyOptionsQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsQuestion"/> class.
        /// </summary>
        public MultyOptionsQuestion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsQuestion"/> class.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public MultyOptionsQuestion(string text)
            : base(text)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add multy attr.
        /// </summary>
        public string AddMultyAttr { get; set; }

        
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <exception cref="DuplicateNameException">
        /// </exception>
        public override void AddAnswer(IAnswer answer)
        {
            if (answer == null)
            {
                return;
            }

            // AddAnswer(answer);
            if (this.Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
            {
                throw new DuplicateNameException("answer with current publick key already exist");
            }

            this.Answers.Add(answer);
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
        /// <exception cref="DuplicateNameException">
        /// </exception>
        /// <exception cref="CompositeException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == this.PublicKey)
            {
                var answer = c as IAnswer;
                if (answer != null)
                {
                    // AddAnswer(answer);
                    if (this.Children.Any(a => a.PublicKey.Equals(answer.PublicKey)))
                    {
                        throw new DuplicateNameException("answer with current publick key already exist");
                    }

                    this.Children.Add(answer);
                    return;
                }
            }

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
            return Enumerable.Empty<T>();
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