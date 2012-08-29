// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using System;

    using RavenQuestionnaire.Core.Utility;

    /// <summary>
    /// The questionnaire browse item.
    /// </summary>
    public class QuestionnaireBrowseItem
    {
        #region Fields

        /// <summary>
        /// The _id.
        /// </summary>
        private string _id;

        #endregion

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
        public QuestionnaireBrowseItem(string id, string title, DateTime creationDate, DateTime lastEntryDate)
        {
            this.Id = id;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
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
        public string Id
        {
            get
            {
                return IdUtil.ParseId(this._id);
            }

            private set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// Gets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; private set; }

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
            return new QuestionnaireBrowseItem(null, null, DateTime.Now, DateTime.Now);
        }

        #endregion
    }
}