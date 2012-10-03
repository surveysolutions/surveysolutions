// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBinded.cs" company="">
//   
// </copyright>
// <summary>
//   The Binded interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The Binded interface.
    /// </summary>
    public interface IBinded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the parent public key.
        /// </summary>
        Guid ParentPublicKey { get; set; }

        #endregion
    }
}