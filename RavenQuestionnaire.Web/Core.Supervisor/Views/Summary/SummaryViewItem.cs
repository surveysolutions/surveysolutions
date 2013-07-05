namespace Core.Supervisor.Views.Summary
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryViewItem"/> class.
        /// </summary>
        public SummaryViewItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryViewItem"/> class.
        /// </summary>
        /// <param name="user">
        /// The user
        /// </param>
        /// <param name="total">
        /// The total.
        /// </param>
        /// <param name="initial">
        /// The initial.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="completed">
        /// The completed.
        /// </param>
        /// <param name="approve">
        /// The approve.
        /// </param>
        /// <param name="redo">
        /// The redo.
        /// </param>
        public SummaryViewItem(
            UserLight user,
            int total,
            int initial,
            int error,
            int completed,
            int approve,
            int redo,
            int unassigned)
            : this()
        {
            this.User = user;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Completed = completed;
            this.Approved = approve;
            this.Redo = redo;
            this.Unassigned = unassigned;
        }

        #endregion

        #region Public Properties

        public int Unassigned { get; set; }
        /// <summary>
        /// Gets or sets Approve.
        /// </summary>
        public int Approved { get; set; }

        /// <summary>
        /// Gets or sets the complete.
        /// </summary>
        public int Completed { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        public int Initial { get; set; }

        /// <summary>
        /// Gets or sets Redo.
        /// </summary>
        public int Redo { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }

        #endregion
    }
}