// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGroup.cs" company="">
//   
// </copyright>
// <summary>
//   The Group interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities
{
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Observers;

    /// <summary>
    /// The Group interface.
    /// </summary>
    public interface IGroup : IComposite, ITriggerable
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        string Description { get; set; }

        #endregion

        // bool Enabled { get; set; }
    }
}