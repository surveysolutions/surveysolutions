namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The FlowBlock interface.
    /// </summary>
    public interface IFlowBlock
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        int Left { get; set; }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        int Top { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        int Width { get; set; }

        #endregion
    }
}