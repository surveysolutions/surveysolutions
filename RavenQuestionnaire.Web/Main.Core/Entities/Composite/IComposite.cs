// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IComposite.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Composite interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.Composite
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Composite interface.
    /// </summary>
    public interface IComposite
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        List<IComposite> Children { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        IComposite Parent { get; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        Guid PublicKey { get; }

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
        void Add(IComposite c, Guid? parent);

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
        T Find<T>(Guid publicKey) where T : class, IComposite;

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
        IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

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
        T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        void Remove(IComposite c);

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        void Remove(Guid publicKey);

        #endregion
    }
}