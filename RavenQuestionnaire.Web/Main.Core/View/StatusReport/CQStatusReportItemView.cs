using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.StatusReport
{
    /// <summary>
    /// The cq status report item view.
    /// </summary>
    public class CQStatusReportItemView
    {
        #region Public Properties

        public CQStatusReportItemView(Guid publicKey, UserLight assignToUser, string description, DateTime lastChangeDate, DateTime lastSyncDate)
        {
            PublicKey = publicKey;
            AssignToUser = assignToUser;
            Description = description;
            LastChangeDate = lastChangeDate;
            LastSyncDate = lastSyncDate;
        }

        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the assign to user.
        /// </summary>
        public UserLight AssignToUser { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the last change date.
        /// </summary>
        public DateTime LastChangeDate { get; set; }

        /// <summary>
        /// Gets or sets the last sync date.
        /// </summary>
        public DateTime LastSyncDate { get; set; }

        #endregion
    }
}