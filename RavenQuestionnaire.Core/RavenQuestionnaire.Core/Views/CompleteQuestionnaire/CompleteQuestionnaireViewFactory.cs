// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;

    using Main.Core.Documents;

    using Raven.Client;

    /// <summary>
    /// The complete questionnaire view factory.
    /// </summary>
    public class CompleteQuestionnaireViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDocumentSession documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.CompleteQuestionnaireView.
        /// </returns>
        public CompleteQuestionnaireView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId != null && input.CompleteQuestionnaireId != Guid.Empty)
            {
                var doc = this.documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);

                return new CompleteQuestionnaireView(doc);
            }

            return null;
        }

        #endregion
    }
}