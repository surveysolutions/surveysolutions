using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Models
{
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