namespace Main.Core.View.SyncProcess
{
    using System;

    using Main.Core.View.User;

    /// <summary>
    /// The user view input model.
    /// </summary>
    public class SyncProcessInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessInputModel"/> class. 
        /// </summary>
        /// <param name="currentUser">
        /// The current User.
        /// </param>
        public SyncProcessInputModel(Guid syncKey, Guid currentUser)
        {
            this.CurrentUserId = currentUser;
            this.SyncKey = syncKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets Current User id.
        /// </summary>
        public Guid CurrentUserId { get; private set; }

        /// <summary>
        /// Gets or sets SyncKey.
        /// </summary>
        public Guid SyncKey { get; set; }

        #endregion
    }
}