// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAutoPropagate.cs" company="">
//   
// </copyright>
// <summary>
//   The AutoPropagate interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The AutoPropagate interface.
    /// </summary>
    public interface IAutoPropagate
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        Guid TargetGroupKey { get; set; }

        #endregion
    }
}