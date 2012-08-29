// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssigmentBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assigment browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Assignment
{
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The assigment browse item.
    /// </summary>
    public class AssigmentBrowseItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssigmentBrowseItem"/> class.
        /// </summary>
        public AssigmentBrowseItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssigmentBrowseItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        public AssigmentBrowseItem(string id, SurveyStatus status, string templateId, int count, UserLight responsible)
        {
            this.Id = id;
            this.Status = status;
            this.TemplateId = templateId;
            this.Count = count;
            this.Responsible = responsible;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public SurveyStatus Status { get; private set; }

        /// <summary>
        /// Gets the template id.
        /// </summary>
        public string TemplateId { get; private set; }

        #endregion
    }
}