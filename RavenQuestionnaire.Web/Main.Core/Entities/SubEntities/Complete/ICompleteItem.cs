// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompleteItem interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The CompleteItem interface.
    /// </summary>
    public interface ICompleteItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        Guid? PropogationPublicKey { get; set; }
    }
}
