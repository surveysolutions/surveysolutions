// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireEnumerableViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire enumerable view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete questionnaire enumerable view factory.
    /// </summary>
    public class CompleteQuestionnaireEnumerableViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentItemSession;

        /// <summary>
        /// The group factory.
        /// </summary>
        private readonly ICompleteGroupFactory groupFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireEnumerableViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        /// <param name="groupFactory">
        /// The group factory.
        /// </param>
        public CompleteQuestionnaireEnumerableViewFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentItemSession, 
            ICompleteGroupFactory groupFactory)
        {
            this.documentItemSession = documentItemSession;
            this.groupFactory = groupFactory;
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.CompleteQuestionnaireViewEnumerable.
        /// </returns>
        public CompleteQuestionnaireViewEnumerable Load(CompleteQuestionnaireViewInputModel input)
        {
            if (Guid.Empty != input.CompleteQuestionnaireId)
            {
                CompleteQuestionnaireStoreDocument doc =
                    this.documentItemSession.Query().FirstOrDefault(i => i.PublicKey == input.CompleteQuestionnaireId);
                ICompleteGroup group = null;

                // Iterator<ICompleteGroup> iterator = new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
                }

                return new CompleteQuestionnaireViewEnumerable(doc, group, this.groupFactory);
            }

            return null;
        }

        #endregion
    }
}