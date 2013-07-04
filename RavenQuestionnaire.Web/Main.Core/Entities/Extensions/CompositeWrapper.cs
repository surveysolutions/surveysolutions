namespace Main.Core.Entities.Extensions
{
    using System;

    using Main.Core.Entities.Composite;

    /// <summary>
    /// The composite wrapper.
    /// </summary>
    internal class CompositeWrapper
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeWrapper"/> class.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="parentKey">
        /// The parent key.
        /// </param>
        public CompositeWrapper(IComposite node, Guid? parentKey)
        {
            this.Node = node;
            this.ParentKey = parentKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the node.
        /// </summary>
        public IComposite Node { get; private set; }

        /// <summary>
        /// Gets the parent key.
        /// </summary>
        public Guid? ParentKey { get; private set; }

        #endregion
    }
}
