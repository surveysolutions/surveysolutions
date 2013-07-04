namespace Main.Core.Entities.SubEntities.Question
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The single question.
    /// </summary>
    public class SingleQuestion : AbstractQuestion, ISingleQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleQuestion"/> class.
        /// </summary>
        public SingleQuestion()
        {
            this.Children = new List<IComposite>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleQuestion"/> class.
        /// </summary>
        /// <param name="qid">
        /// The qid.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public SingleQuestion(Guid qid, string text) : base(text)
        {
            this.PublicKey = qid;
            this.Children = new List<IComposite>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add single attr.
        /// </summary>
        public string AddSingleAttr { get; set; }
        
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

            if (this.Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
            {
                throw new DuplicateNameException("answer with current public key already exist");
            }

            this.Answers.Add(answer);
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