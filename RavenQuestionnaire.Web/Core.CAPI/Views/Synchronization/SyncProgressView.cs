using System;
using System.Linq;
using Main.Core.Documents;

namespace Core.CAPI.Views.Synchronization
{
    /// <summary>
    /// The sync progress view.
    /// </summary>
    public class SyncProgressView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProgressView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public SyncProgressView(SyncProcessDocument doc)
        {
            this.ProcessPublicKey = doc.PublicKey;
            this.StartDate = doc.StartDate;
            this.EndDate = doc.EndDate;
            switch (doc.Handled)
            {
                case EventState.Initial:
                    this.StateDescription = "Handshake";
                    this.ProgressPercentage = 0;
                    break;
                case EventState.Error:
                    this.StateDescription = "Process is finished with errors";
                    this.ProgressPercentage = -1;
                    break;
                case EventState.Completed:
                    this.StateDescription = "Process is finished";
                    this.ProgressPercentage = 100;
                    break;
                case EventState.InProgress:
                    this.StateDescription = "Retrieving documents";
                    int initialStateEventsCount = doc.Chunks.Count(e => e.Handled == EventState.Initial);
                    this.ProgressPercentage = doc.Chunks.Count == 0
                                                  ? 100
                                                  : (initialStateEventsCount * 100 / doc.Chunks.Count);

                    // (((decimal)(doc.Chunks.Count - initialStateEventsCount) / doc.Chunks.Count) * 100);
                    // process can't display 100% when it's not marked as completed
                    if (this.ProgressPercentage == 100)
                    {
                        this.ProgressPercentage = 99;
                    }

                    break;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the process public key.
        /// </summary>
        public Guid ProcessPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the progress percentage.
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the state description.
        /// </summary>
        public string StateDescription { get; set; }

        #endregion
    }
}