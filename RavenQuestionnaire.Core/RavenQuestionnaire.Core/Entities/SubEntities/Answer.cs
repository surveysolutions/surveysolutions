// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Answer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Answer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using RavenQuestionnaire.Core.Entities.Composite;

    /// <summary>
    /// The Answer interface.
    /// </summary>
    public interface IAnswer : IComposite
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer image.
        /// </summary>
        string AnswerImage { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the answer type.
        /// </summary>
        AnswerType AnswerType { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        object AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the name collection.
        /// </summary>
        string NameCollection { get; set; }

        #endregion
    }

    /// <summary>
    /// The answer.
    /// </summary>
    public class Answer : IAnswer
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Answer"/> class.
        /// </summary>
        public Answer( /*Question owner*/)
        {
            this.PublicKey = Guid.NewGuid();

            // QuestionId = owner.QuestionId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer image.
        /// </summary>
        public string AnswerImage { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        public string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the answer type.
        /// </summary>
        public AnswerType AnswerType { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        public object AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<IComposite> Children
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
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the name collection.
        /// </summary>
        public string NameCollection { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        [JsonIgnore]
        public IComposite Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion

        // public string QuestionId { get; set; }
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
        /// <exception cref="CompositeException">
        /// </exception>
        public void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException("answer is not hierarchical");
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
        /// <exception cref="CompositeException">
        /// </exception>
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            throw new CompositeException("answer is not hierarchical");
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
        /// <exception cref="CompositeException">
        /// </exception>
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            throw new CompositeException("answer is not hierarchical");
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
        /// <exception cref="CompositeException">
        /// </exception>
        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            throw new CompositeException("answer is not hierarchical");
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public void Remove(IComposite c)
        {
            throw new CompositeException("answer is not hierarchical");
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        public void Remove(Guid publicKey)
        {
            throw new CompositeException("answer is not hierarchical");
        }

        #endregion
    }
}