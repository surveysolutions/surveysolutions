namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The flow connection.
    /// </summary>
    public class FlowConnection
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowConnection"/> class.
        /// </summary>
        public FlowConnection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowConnection"/> class.
        /// </summary>
        /// <param name="sourceId">
        /// The source id.
        /// </param>
        /// <param name="targetId">
        /// The target id.
        /// </param>
        public FlowConnection(Guid sourceId, Guid targetId)
        {
            this.Source = sourceId;
            this.Target = targetId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        public string LabelText { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public Guid Source { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public Guid Target { get; set; }

        #endregion
    }
}