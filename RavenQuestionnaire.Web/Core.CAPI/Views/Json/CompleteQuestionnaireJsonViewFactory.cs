// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireJsonViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire json view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Core.CAPI.Views.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.Group;

    /// <summary>
    /// The complete questionnaire json view factory.
    /// </summary>
    public class CompleteQuestionnaireJsonViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireJsonViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public CompleteQuestionnaireJsonViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json.CompleteQuestionnaireJsonView.
        /// </returns>
        public CompleteQuestionnaireJsonView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId != Guid.Empty)
            {
                CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);
                ICompleteGroup group = null;
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.FindGroupByKey(input.CurrentGroupPublicKey.Value, input.PropagationKey);
                }

                return new CompleteQuestionnaireJsonView(doc, group, input.Scope);
            }

            return null;
        }

        #endregion
    }

}