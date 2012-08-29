// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompositeView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompositeView interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views
{
    using System;
    using System.Collections.Generic;

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