using System;
using System.Collections.Generic;

namespace Main.Core.Entities.Observers
{
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