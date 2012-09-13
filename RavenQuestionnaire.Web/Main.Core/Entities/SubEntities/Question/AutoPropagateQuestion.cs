// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPropagateQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The auto propagate question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Question
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The auto propagate question.
    /// </summary>
    public class AutoPropagateQuestion : AbstractQuestion, IAutoPropagate
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateQuestion"/> class.
        /// </summary>
        public AutoPropagateQuestion()
        {
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
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the add numeric attr.
        /// </summary>
        public string AddNumericAttr { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public override List<IComposite> Children
        {
            get
            {
                return new List<IComposite>(0);
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the int attr.
        /// </summary>
        public int IntAttr { get; set; }

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        public Guid TargetGroupKey { get; set; }

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
        /// <exception cref="CompositeException">
        /// </exception>
        public override void Add(IComposite c, Guid? parent)
        {
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
        }

        #endregion
    }
}