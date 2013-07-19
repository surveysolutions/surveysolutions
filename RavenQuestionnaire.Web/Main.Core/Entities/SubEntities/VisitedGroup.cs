namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The visited group.
    /// </summary>
    public class VisitedGroup
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VisitedGroup"/> class.
        /// </summary>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public VisitedGroup(Guid groupKey, Guid? propagationKey)
        {
            this.GroupKey = groupKey;
            this.PropagationKey = propagationKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the group key.
        /// </summary>
        public Guid GroupKey { get; private set; }

        /// <summary>
        /// Gets the propagation key.
        /// </summary>
        public Guid? PropagationKey { get; private set; }

        #endregion
    }
}