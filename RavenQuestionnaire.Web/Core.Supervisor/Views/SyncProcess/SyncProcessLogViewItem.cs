namespace Core.Supervisor.Views.SyncProcess
{
    using System;

    using Main.Core.Documents;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessLogViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessLogViewItem"/> class.
        /// </summary>
        /// <param name="process">
        /// The process.
        /// </param>
        public SyncProcessLogViewItem(SyncProcessStatisticsDocument process)
        {
            this.Description = process.Description;
            this.StartDate = process.CreationDate;
            this.EndDate = process.EndDate;
            this.SyncType = process.SyncType;
            this.ExitDescription = process.ExitDescription;
            this.PubliKey = process.PublicKey;
            this.ParentKey = process.ParentProcessKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets EndDate.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets ExitDescription.
        /// </summary>
        public string ExitDescription { get; set; }

        /// <summary>
        /// Gets or sets ParentKey.
        /// </summary>
        public Guid? ParentKey { get; set; }

        /// <summary>
        /// Gets or sets PubliKey.
        /// </summary>
        public Guid PubliKey { get; set; }

        /// <summary>
        /// Gets or sets StartDate.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets SyncType.
        /// </summary>
        public SynchronizationType SyncType { get; set; }

        #endregion
    }
}