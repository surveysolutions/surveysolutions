namespace Core.Supervisor.Views.SyncProcess
{
    using System.Collections.Generic;

    using Main.Core.Documents;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessLogView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessLogView"/> class.
        /// </summary>
        /// <param name="processes">
        /// The processes.
        /// </param>
        public SyncProcessLogView(IEnumerable<SyncProcessStatisticsDocument> processes)
        {
            this.Items = new List<SyncProcessLogViewItem>();
            foreach (SyncProcessStatisticsDocument process in processes)
            {
                this.Items.Add(new SyncProcessLogViewItem(process));
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<SyncProcessLogViewItem> Items { get; set; }

        #endregion
    }
}