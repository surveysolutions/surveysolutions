// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSyncProcessStatistics.cs" company="The World Bank">
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
    public class UserSyncProcessStatistics
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

        /// <summary>
        /// Gets or sets PrevStatus.
        /// </summary>
        public SurveyStatus PrevStatus { get; set; }

        /// <summary>
        /// Gets or sets PrevUser.
        /// </summary>
        public UserLight PrevUser { get; set; }

        /// <summary>
        /// Gets or sets message Type.
        /// </summary>
        public SynchronizationStatisticType Type { get; set; }

        /// <summary>
        /// Gets or sets amount of duplicates
        /// </summary>
        public int Count { get; set; }

        #endregion
    }
}