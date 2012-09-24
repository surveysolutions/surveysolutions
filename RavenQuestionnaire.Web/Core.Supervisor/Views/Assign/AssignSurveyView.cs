// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignSurveyView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assign survey view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;

namespace Core.Supervisor.Views.Assign
{
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
        public AssignSurveyView(ICompleteQuestionnaireDocument completeQuestionnaire)
        {
            this.Id = completeQuestionnaire.PublicKey;
            this.QuestionnaireTitle = completeQuestionnaire.Title;
            this.TemplateId = completeQuestionnaire.TemplateId;
            this.Status = completeQuestionnaire.Status;
            this.Responsible = completeQuestionnaire.Responsible;
            this.FeaturedQuestions = new List<CompleteQuestionView>();
            foreach (ICompleteQuestion q in completeQuestionnaire.QuestionHash.Questions.Where(q => q.Featured))
            {
                //     var question = completeQuestionnaire.Find<ICompleteQuestion>(q.PublicKey);
                var questionView = new CompleteQuestionView(completeQuestionnaire, q);
                //  questionView.ParentGroupPublicKey = q.GroupPublicKey;
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