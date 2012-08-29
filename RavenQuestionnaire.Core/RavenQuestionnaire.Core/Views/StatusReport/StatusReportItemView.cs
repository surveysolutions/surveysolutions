// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusReportItemView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The status report item view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System;

    /// <summary>
    /// The status report item view.
    /// </summary>
    public class StatusReportItemView
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire count.
        /// </summary>
        public int QuestionnaireCount { get; set; }

        /// <summary>
        /// Gets or sets the status id.
        /// </summary>
        public Guid StatusId { get; set; }

        /// <summary>
        /// Gets or sets the status title.
        /// </summary>
        public string StatusTitle { get; set; }

        #endregion
    }
}