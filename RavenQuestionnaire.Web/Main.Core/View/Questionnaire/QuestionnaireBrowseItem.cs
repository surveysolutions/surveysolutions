// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.View.Questionnaire
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities;

    /// <summary>
    /// The questionnaire browse item.
    /// </summary>
    public class QuestionnaireBrowseItem
    {
        #region Constructors and Destructors
        public QuestionnaireBrowseItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="creationDate">
        /// The creation date.
        /// </param>
        /// <param name="lastEntryDate">
        /// The last entry date.
        /// </param>
        /// <param name="createdBy">
        /// The created by.
        /// </param>
        public QuestionnaireBrowseItem(Guid questionnaireId, string title, DateTime creationDate, DateTime lastEntryDate, Guid? createdBy)
        {
            this.QuestionnaireId = questionnaireId;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.CreatedBy = createdBy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public QuestionnaireBrowseItem(QuestionnaireDocument doc)
            : this(doc.PublicKey, doc.Title, doc.CreationDate, doc.LastEntryDate, doc.CreatedBy)
        {
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid QuestionnaireId { get; private set; }

        /// <summary>
        /// Gets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; private set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get;  set; }

        /// <summary>
        /// Gets the created by.
        /// </summary>
        public Guid? CreatedBy { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The new.
        /// </summary>
        /// <returns>
        /// The QuestionnaireBrowseItem.
        /// </returns>
        public static QuestionnaireBrowseItem New()
        {
            return new QuestionnaireBrowseItem(Guid.Empty, null, DateTime.Now, DateTime.Now, null);
        }

        /// <summary>
        /// Get Template Light
        /// </summary>
        /// <returns>
        /// GetTemplateLight object
        /// </returns>
        public TemplateLight GetTemplateLight()
        {
            return new TemplateLight(this.QuestionnaireId, this.Title);
        }

        #endregion
    }
}