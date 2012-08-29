// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The single question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities.Question
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

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
        public SingleQuestion(Guid qid, string text)
            : base(text)
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
            if (typeof(T).IsAssignableFrom(typeof(IAnswer)))
            {
                return this.Children.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
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
            return this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T);
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
            return this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault();
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
            if (this.Children.RemoveAll(a => a.PublicKey.Equals(publicKey)) > 0)
            {
                return;
            }

            throw new CompositeException();
        }

        #endregion
    }
}