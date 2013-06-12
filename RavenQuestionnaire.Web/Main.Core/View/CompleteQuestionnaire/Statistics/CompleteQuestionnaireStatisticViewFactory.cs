// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireStatisticViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire statistic view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Main.Core.View.CompleteQuestionnaire.Statistics
{
    using System;

    using Main.Core.Documents;
    using Main.DenormalizerStorage;

    /// <summary>
    /// The complete questionnaire statistic view factory.
    /// </summary>
    public class CompleteQuestionnaireStatisticViewFactory :
        IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireStatisticViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public CompleteQuestionnaireStatisticViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
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
        /// The RavenQuestionnaire.Core.Views.Statistics.CompleteQuestionnaireStatisticView.
        /// </returns>
        public CompleteQuestionnaireStatisticView Load(CompleteQuestionnaireStatisticViewInputModel input)
        {
            CompleteQuestionnaireStoreDocument doc = this.store.GetById(input.Id);
            return new CompleteQuestionnaireStatisticView(doc, input.Scope);
        }

        #endregion
    }
}