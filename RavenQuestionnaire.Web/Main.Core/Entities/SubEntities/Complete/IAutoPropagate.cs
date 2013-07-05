using Main.Core.Entities.Observers;

namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The AutoPropagate interface.
    /// </summary>
    public interface IAutoPropagate : ITriggerable
    {
        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        int MaxValue { get; set; }
    }
}