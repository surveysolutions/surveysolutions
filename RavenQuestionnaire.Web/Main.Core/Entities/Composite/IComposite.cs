namespace Main.Core.Entities.Composite
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Composite interface.
    /// </summary>
    public interface IComposite /*: ICloneable*/
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        List<IComposite> Children { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        IComposite GetParent();

        /// <summary>
        /// Gets or sets the parent.
        /// IS USED TO AVOID SERIALIZATION 
        /// </summary>
        void SetParent(IComposite parent);

        /// <summary>
        /// Gets the public key.
        /// </summary>
        Guid PublicKey { get; }

        #endregion

        #region Public Methods and Operators
        
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
        /// The connect childs with parent.
        /// </summary>
        void ConnectChildsWithParent();


        // Could be created as a separate interface
        // but we need cast an object every time 

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        IComposite Clone();

        #endregion
    }
}