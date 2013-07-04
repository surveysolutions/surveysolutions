namespace Core.CAPI.Views
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
        /// <param name="ifNew">
        /// The if new.
        /// </param>
        public SyncStatisticInfo(
            string interviewersName, int assignments, int approvedQuestionaries, int rejectQuestionaries, bool ifNew)
        {
            this.InterviewersName = interviewersName;
            this.Assignments = assignments;
            this.ApprovedQuestionaries = approvedQuestionaries;
            this.RejectQuestionaries = rejectQuestionaries;
            this.New = ifNew;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets ApprovedQuestionaries.
        /// </summary>
        public int ApprovedQuestionaries { get; set; }

        /// <summary>
        /// Gets Assignments.
        /// </summary>
        public int Assignments { get; set; }

        /// <summary>
        /// Gets InterviewersName.
        /// </summary>
        public string InterviewersName { get; set; }

        /// <summary>
        /// Gets a value indicating whether New.
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        /// Gets RejectQuestionaries.
        /// </summary>
        public int RejectQuestionaries { get; set; }

        #endregion
    }
}