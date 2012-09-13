// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteGroup.cs" company="">
//   
// </copyright>
// <summary>
//   The CompleteGroup interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The CompleteGroup interface.
    /// </summary>
    public interface ICompleteGroup : IGroup
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        Guid? PropogationPublicKey { get; set; }

        #endregion
    }
}