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

        #endregion

        #region Public Properties

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