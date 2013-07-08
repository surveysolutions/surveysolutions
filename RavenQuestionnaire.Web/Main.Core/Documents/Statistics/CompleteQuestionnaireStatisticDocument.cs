namespace Main.Core.Documents.Statistics
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The complete questionnaire statistic document.
    /// </summary>
    public class CompleteQuestionnaireStatisticDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireStatisticDocument"/> class.
        /// </summary>
        public CompleteQuestionnaireStatisticDocument()
        {
            this.AnsweredQuestions = new List<QuestionStatisticDocument>();
            this.InvalidQuestions = new List<QuestionStatisticDocument>();
            this.FeturedQuestions = new List<QuestionStatisticDocument>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answered questions.
        /// </summary>
        public IList<QuestionStatisticDocument> AnsweredQuestions { get; set; }

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the fetured questions.
        /// </summary>
        public IList<QuestionStatisticDocument> FeturedQuestions { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the invalid questions.
        /// </summary>
        public IList<QuestionStatisticDocument> InvalidQuestions { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the total question count.
        /// </summary>
        public int TotalQuestionCount { get; set; }

        #endregion
    }
}