// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Main.Core.Documents;
using Main.Core.Documents.Statistics;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.View.CompleteQuestionnaire
{
    /// <summary>
    /// The complete questionnaire browse item.
    /// </summary>
    public class CompleteQuestionnaireBrowseItem : IView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The complete questionnaire.
        /// </param>
        public CompleteQuestionnaireBrowseItem(ICompleteQuestionnaireDocument doc)
            : this()
        {
            this.CompleteQuestionnaireId = doc.PublicKey;
            this.TemplateId = doc.TemplateId;
            this.QuestionnaireTitle = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.TotalQuestionCount = 0;
            this.AnsweredQuestionCount = 0;
            this.Responsible = doc.Responsible;
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
            this.FeaturedQuestions = new CompleteQuestionView[0];
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
        public CompleteQuestionView[] FeaturedQuestions { get; set; }

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