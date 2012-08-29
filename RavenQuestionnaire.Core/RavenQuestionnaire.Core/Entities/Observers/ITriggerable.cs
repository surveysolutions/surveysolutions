// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITriggerable.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Triggerable interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.Observers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Triggerable interface.
    /// </summary>
    public interface ITriggerable
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the triggers.
        /// </summary>
        List<Guid> Triggers { get; set; }

        #endregion
    }
}