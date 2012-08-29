// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;

    /// <summary>
    /// The questionnaire view factory.
    /// </summary>
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public QuestionnaireViewFactory(IDenormalizerStorage<QuestionnaireDocument> documentSession)
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
        /// The RavenQuestionnaire.Core.Views.Questionnaire.QuestionnaireView.
        /// </returns>
        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            QuestionnaireDocument doc = this.documentSession.GetByGuid(input.QuestionnaireId);

            return new QuestionnaireView(doc);
        }

        #endregion
    }
}