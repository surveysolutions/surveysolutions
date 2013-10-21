using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.SyncProcess
{
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