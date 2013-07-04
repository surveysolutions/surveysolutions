using Main.Core.View.CompleteQuestionnaire.Statistics;

namespace Web.Supervisor.Models
{
    using System;

    /// <summary>
    /// Define Approve model
    /// </summary>
    public class ApproveRedoModel
    {
        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets Comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets Statistic.
        /// </summary>
        public CompleteQuestionnaireStatisticView Statistic { get; set; }

        /// <summary>
        /// Gets or sets Statuses.
        /// </summary>
        public Guid StatusId { get; set; }
    }
}