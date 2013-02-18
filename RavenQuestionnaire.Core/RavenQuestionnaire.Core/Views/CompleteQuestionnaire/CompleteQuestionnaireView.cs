// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    using RavenQuestionnaire.Core.Views.Group;
    using RavenQuestionnaire.Core.Views.Questionnaire;

    /// <summary>
    /// The complete questionnaire view.
    /// </summary>
    public class CompleteQuestionnaireView : AbstractQuestionnaireView<CompleteGroupView, CompleteQuestionView>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireView"/> class.
        /// </summary>
        public CompleteQuestionnaireView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireView(IQuestionnaireDocument doc)
            : base(doc)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireView(CompleteQuestionnaireStoreDocument doc)
            : base(doc)
        {
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
          
            this.TemplateId = doc.TemplateId;
        }

        #endregion

        #region Public Properties

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