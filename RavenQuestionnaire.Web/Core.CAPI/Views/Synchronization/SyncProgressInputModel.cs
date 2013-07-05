using System;

namespace Core.CAPI.Views.Synchronization
{
    /// <summary>
    /// The sync progress input model.
    /// </summary>
    public class SyncProgressInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProgressInputModel"/> class.
        /// </summary>
        /// <param name="progressKey">
        /// The progress key.
        /// </param>
        public SyncProgressInputModel(Guid progressKey)
        {
            this.ProcessKey = progressKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the process key.
        /// </summary>
        public Guid ProcessKey { get; private set; }

        #endregion
    }
}