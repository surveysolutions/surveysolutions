// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssigmentModel.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the AssigmentModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// Define aasigment model
    /// </summary>
    public class AssigmentModel
    {
        /// <summary>
        /// Gets or sets ColumnsCount.
        /// </summary>
        public int ColumnsCount { get; set; }

        /// <summary>
        /// Gets or sets Responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets CompleteQuestionnaireId.
        /// </summary>
        public string CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets FeaturedCount.
        /// </summary>
        public int FeaturedCount { get; set; }
    }
}