// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusReportView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The status report view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System.Collections.Generic;

    /// <summary>
    /// The status report view.
    /// </summary>
    public class StatusReportView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusReportView"/> class.
        /// </summary>
        public StatusReportView()
        {
            this.Items = new List<StatusReportGroupView>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<StatusReportGroupView> Items { get; set; }

        #endregion
    }
}