// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignSurveyView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assign survey view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Assign
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
    using RavenQuestionnaire.Core.Views.Question;
    using RavenQuestionnaire.Core.Views.Statistics;

    /// <summary>
    /// The assign survey view.
    /// </summary>
    public class AssignSurveyView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignSurveyView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="completeQuestionnaire">
        /// The complete questionnaire.
        /// </param>
        public AssignSurveyView(
            CompleteQuestionnaireBrowseItem doc, CompleteQuestionnaireStoreDocument completeQuestionnaire)
        {
            this.Id = completeQuestionnaire.PublicKey;
            this.QuestionnaireTitle = doc.QuestionnaireTitle;
            this.TemplateId = doc.TemplateId;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            this.FeaturedQuestions = new List<CompleteQuestionView>();
            foreach (QuestionStatisticView q in doc.FeaturedQuestions)
            {
                var question = completeQuestionnaire.Find<ICompleteQuestion>(q.PublicKey);
                var questionView = new CompleteQuestionView(completeQuestionnaire, question);
                questionView.ParentGroupPublicKey = q.GroupPublicKey;
                this.FeaturedQuestions.Add(questionView);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the featured questions.
        /// </summary>
        public List<CompleteQuestionView> FeaturedQuestions { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

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

        #endregion
    }
}