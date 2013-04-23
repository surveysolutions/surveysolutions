// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncronizationStatus.cs" company="">
//   
// </copyright>
// <summary>
//   The syncronization status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SyncManager
{
    /// <summary>
    /// The syncronization status.
    /// </summary>
    public class SyncronizationStatus
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the current stage description.
        /// </summary>
        public string CurrentStageDescription { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is working.
        /// </summary>
        public bool IsWorking { get; set; }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether result.
        /// </summary>
        public bool Result { get; set; }

        public SyncronizationStatus()
        {
            IsWorking = true;
            Progress = 0;
        }


        #endregion
    }
}