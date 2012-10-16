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

    /// <summary>
    /// The questionnaire browse item.
    /// </summary>
    public class QuestionnaireBrowseItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="id">
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
        public QuestionnaireBrowseItem(Guid id, string title, DateTime creationDate, DateTime lastEntryDate)
        {
            this.Id = id;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public QuestionnaireBrowseItem(QuestionnaireDocument doc)
            : this(doc.PublicKey, doc.Title, doc.CreationDate, doc.LastEntryDate)
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
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get;  set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The new.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Questionnaire.QuestionnaireBrowseItem.
        /// </returns>
        public static QuestionnaireBrowseItem New()
        {
            return new QuestionnaireBrowseItem(Guid.Empty, null, DateTime.Now, DateTime.Now);
        }

        #endregion
    }
}