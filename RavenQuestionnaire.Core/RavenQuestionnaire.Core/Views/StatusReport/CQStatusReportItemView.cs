// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQStatusReportItemView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq status report item view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The cq status report item view.
    /// </summary>
    public class CQStatusReportItemView
    {
        #region Public Properties

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