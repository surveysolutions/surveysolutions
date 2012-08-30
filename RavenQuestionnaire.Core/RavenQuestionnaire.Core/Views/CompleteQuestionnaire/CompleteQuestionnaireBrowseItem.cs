// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;

    using RavenQuestionnaire.Core.Documents.Statistics;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.Statistics;

    /// <summary>
    /// The complete questionnaire browse item.
    /// </summary>
    public class CompleteQuestionnaireBrowseItem
    {
        #region Fields

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        /// <param name="questionnaireTitle">
        /// The questionnaire title.
        /// </param>
        /// <param name="creationDate">
        /// The creation date.
        /// </param>
        /// <param name="lastEntryDate">
        /// The last entry date.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="totalQuestionCount">
        /// The total question count.
        /// </param>
        /// <param name="answeredQuestionCount">
        /// The answered question count.
        /// </param>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        public CompleteQuestionnaireBrowseItem(
            Guid completeQuestionnaireId, 
            Guid templateId, 
            string questionnaireTitle, 
            DateTime creationDate, 
            DateTime lastEntryDate, 
            SurveyStatus status, 
            int totalQuestionCount, 
            int answeredQuestionCount, 
            UserLight responsible)
            : this()
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.TemplateId = templateId;
            this.QuestionnaireTitle = questionnaireTitle;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Status = status;
            this.TotalQuestionCount = totalQuestionCount;
            this.AnsweredQuestionCount = answeredQuestionCount;
            this.Responsible = responsible;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireBrowseItem(CompleteQuestionnaireStatisticDocument doc)
            : this()
        {
            this.CompleteQuestionnaireId = doc.CompleteQuestionnaireId;
            this.QuestionnaireTitle = doc.Title;
            this.CreationDate = doc.StartDate;
            this.LastEntryDate = doc.EndDate ?? DateTime.Now;
            this.Status = doc.Status;
            this.TotalQuestionCount = doc.TotalQuestionCount;
            this.AnsweredQuestionCount = doc.AnsweredQuestions.Count;
            this.Creator = doc.Creator;

            // this.FeaturedQuestions = doc.FeturedQuestions.Select(q => new QuestionStatisticView(q)).ToArray();
            // this.Responsible = doc.r;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseItem"/> class.
        /// </summary>
        protected CompleteQuestionnaireBrowseItem()
        {
            this.FeaturedQuestions = new QuestionStatisticView[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answered question count.
        /// </summary>
        public int AnsweredQuestionCount { get; set; }

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the featured questions.
        /// </summary>
        public QuestionStatisticView[] FeaturedQuestions { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire title.
        /// </summary>
        public string QuestionnaireTitle { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the total question count.
        /// </summary>
        public int TotalQuestionCount { get; set; }

        #endregion
    }
}