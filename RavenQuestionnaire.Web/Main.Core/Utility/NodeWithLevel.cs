using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.Utility
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NodeWithLevel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeWithLevel"/> class.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        public NodeWithLevel(ICompleteGroup group, int level)
        {
            this.Group = group;
            this.Level = level;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the group.
        /// </summary>
        public ICompleteGroup Group { get; private set; }

        /// <summary>
        /// Gets the level.
        /// </summary>
        public int Level { get; private set; }

        #endregion
    }

}
