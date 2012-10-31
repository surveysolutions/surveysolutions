// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The complete questionnaire export item.
    /// </summary>
    public class CompleteQuestionnaireExportItem
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireExportItem(CompleteQuestionnaireDocument doc)
        {
            this.CompleteQuestionnaireKey = doc.PublicKey;
            this.CompleteAnswers = doc.Find<ICompleteAnswer>(a => a.Selected).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportItem"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public CompleteQuestionnaireExportItem(CompleteQuestionnaireStoreDocument document)
        {
            this.CompleteQuestionnaireKey = document.PublicKey;
            this.CompleteQuestions = document.Questions.ToArray();
        }

        #endregion

        #region Public Properties

        public ICompleteQuestion[] CompleteQuestions { get; set; }

        /// <summary>
        /// Gets or sets the complete answers.
        /// </summary>
        public ICompleteAnswer[] CompleteAnswers { get; set; }

        /// <summary>
        /// Gets the complete questionnaire key.
        /// </summary>
        public Guid CompleteQuestionnaireKey { get; private set; }

        #endregion
    }
}