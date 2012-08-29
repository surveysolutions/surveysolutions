// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignSurveyViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assign survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Assign
{
    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

    /// <summary>
    /// The assign survey view factory.
    /// </summary>
    public class AssignSurveyViewFactory : IViewFactory<AssignSurveyInputModel, AssignSurveyView>
    {
        #region Fields

        /// <summary>
        /// The docs.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> docs;

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignSurveyViewFactory"/> class.
        /// </summary>
        /// <param name="docs">
        /// The docs.
        /// </param>
        /// <param name="store">
        /// The store.
        /// </param>
        public AssignSurveyViewFactory(
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> docs, 
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
            this.docs = docs;
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
        /// The RavenQuestionnaire.Core.Views.Assign.AssignSurveyView.
        /// </returns>
        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            CompleteQuestionnaireStoreDocument q = this.store.GetByGuid(input.CompleteQuestionnaireId);
            CompleteQuestionnaireBrowseItem doc = this.docs.GetByGuid(input.CompleteQuestionnaireId);

            return new AssignSurveyView(doc, q);
        }

        #endregion
    }
}