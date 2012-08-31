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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InterviewerStatistics
    {
        /// <summary>
        /// Gets or sets CQ Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }
    }
}
