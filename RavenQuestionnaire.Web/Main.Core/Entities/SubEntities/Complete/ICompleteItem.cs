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
        /// Gets or sets the enable state calculated.
        /// </summary>
        DateTime EnableStateCalculated { get; set; }
        
        /// <summary>
        /// Gets or sets the propagation public key.
        /// </summary>
        Guid? PropagationPublicKey { get; set; }
    }
}
