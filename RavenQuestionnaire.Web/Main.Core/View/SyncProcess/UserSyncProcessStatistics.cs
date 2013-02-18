// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CapiExportStatistics.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.View.SyncProcess
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// User sync process statistics
    /// </summary>
    public class CapiExportStatistics
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets SurveyId.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        #endregion
    }
}