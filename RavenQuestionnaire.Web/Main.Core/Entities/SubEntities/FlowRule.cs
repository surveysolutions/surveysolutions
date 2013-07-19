namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// Is used for holding rule of status changing.
    /// </summary>
    public class FlowRule
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRule"/> class.
        /// </summary>
        public FlowRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRule"/> class.
        /// </summary>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <param name="changeComment">
        /// The change comment.
        /// </param>
        /// <param name="targetStatus">
        /// The target status.
        /// </param>
        public FlowRule(string conditionExpression, string changeComment, SurveyStatus targetStatus)
        {
            this.ConditionExpression = conditionExpression;
            this.TargetStatus = targetStatus;
            this.ChangeComment = changeComment;
            this.Enabled = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the change comment.
        /// </summary>
        public string ChangeComment { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the status id.
        /// </summary>
        public string StatusId { get; set; }

        /// <summary>
        /// Gets or sets the target status.
        /// </summary>
        public SurveyStatus TargetStatus { get; set; }

        #endregion
    }
}