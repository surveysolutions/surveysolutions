// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusReportGroupView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The status report group view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The status report group view.
    /// </summary>
    public class StatusReportGroupView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusReportGroupView"/> class.
        /// </summary>
        public StatusReportGroupView()
        {
            this.Items = new List<StatusReportItemView>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusReportGroupView"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        public StatusReportGroupView(string id, string title)
            : this()
        {
            this.Id = id;
            this.Title = title;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<StatusReportItemView> Items { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}