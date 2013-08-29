namespace Main.Core.View.SyncProcess
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncStatisticInfo
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncStatisticInfo"/> class.
        /// </summary>
        public SyncStatisticInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncStatisticInfo"/> class.
        /// </summary>
        /// <param name="interviewersName">
        /// The interviewers name.
        /// </param>
        /// <param name="assignments">
        /// The assignments.
        /// </param>
        /// <param name="approvedQuestionaries">
        /// The approved questionaries.
        /// </param>
        /// <param name="rejectQuestionaries">
        /// The reject questionaries.
        /// </param>
        /// <param name="isNew">
        /// The if new.
        /// </param>
        public SyncStatisticInfo(
            string interviewersName, int assignments, int approvedQuestionaries, int rejectQuestionaries, bool isNew)
        {
            this.UserName = interviewersName;
            this.NewAssignments = assignments;
            this.Approved = approvedQuestionaries;
            this.Rejected = rejectQuestionaries;
            this.IsNew = isNew;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Approved.
        /// </summary>
        public int Approved { get; set; }

        /// <summary>
        /// Gets or sets Assignments.
        /// </summary>
        public int NewAssignments { get; set; }

        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether New.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets Rejected.
        /// </summary>
        public int Rejected { get; set; }

        #endregion
    }
}