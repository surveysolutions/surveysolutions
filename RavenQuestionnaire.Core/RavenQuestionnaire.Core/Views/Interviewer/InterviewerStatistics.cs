// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerStatistics.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewer item view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Interviewer
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InterviewerStatistics
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets CQ Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}