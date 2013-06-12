// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The question view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;

namespace Main.Core.View.Question
{
    using System;

    /// <summary>
    /// The question view factory.
    /// </summary>
    public class QuestionViewFactory : IViewFactory<QuestionViewInputModel, QuestionView>
    {
        #region Fields

        /// <summary>
        /// The _document session.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> _documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public QuestionViewFactory(IDenormalizerStorage<QuestionnaireDocument> documentSession)
        {
            this._documentSession = documentSession;
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
        /// The RavenQuestionnaire.Core.Views.Question.QuestionView.
        /// </returns>
        public QuestionView Load(QuestionViewInputModel input)
        {
            QuestionnaireDocument doc = this._documentSession.GetById(input.QuestionnaireId);

            var question = doc.Find<IQuestion>(input.PublicKey);
            if (question == null)
            {
                if (input.GroupPublicKey != null)
                    return new QuestionView(doc, input.GroupPublicKey);
                return null;
            }

            return new QuestionView(doc, question);
        }

        #endregion
    }
}