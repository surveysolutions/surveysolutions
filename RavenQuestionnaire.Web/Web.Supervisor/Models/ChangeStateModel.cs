using System;
using Main.Core.View.CompleteQuestionnaire.Statistics;

namespace Web.Supervisor.Models
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// Define changestate model
    /// </summary>
    public class ChangeStateModel
    {


        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public string TemplateId { get; set; }
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