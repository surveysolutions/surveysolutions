namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The flow block.
    /// </summary>
    public class FlowBlock : IFlowBlock
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowBlock"/> class.
        /// </summary>
        public FlowBlock()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowBlock"/> class.
        /// </summary>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        public FlowBlock(Guid questionId)
        {
            this.PublicKey = questionId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }

        #endregion
    }
}