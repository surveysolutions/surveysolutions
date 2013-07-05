using System;
using System.Collections.Generic;

namespace Main.Core.View
{
    /// <summary>
    /// The CompositeView interface.
    /// </summary>
    public interface ICompositeView
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        Guid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        string Title { get; set; }

        #endregion
    }
}