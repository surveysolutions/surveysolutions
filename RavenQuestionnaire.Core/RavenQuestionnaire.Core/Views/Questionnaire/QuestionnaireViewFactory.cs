// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.DenormalizerStorage;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using Main.Core.Documents;

    /// <summary>
    /// The questionnaire view factory.
    /// </summary>
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>,
        IViewFactory<QuestionnaireViewInputModel, QuestionnaireStataMapView>
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
        /// The QuestionnaireView.
        /// </returns>
        QuestionnaireView IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>.Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);

            return new QuestionnaireView(doc);
        }

        #endregion

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The QuestionnaireStataMapView
        /// </returns>
        QuestionnaireStataMapView IViewFactory<QuestionnaireViewInputModel, QuestionnaireStataMapView>.Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);

            return new QuestionnaireStataMapView(doc);
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireViewInputModel input)
        {
            QuestionnaireDocument doc = this.documentSession.GetById(input.QuestionnaireId);
            return doc;
        }
    }
}