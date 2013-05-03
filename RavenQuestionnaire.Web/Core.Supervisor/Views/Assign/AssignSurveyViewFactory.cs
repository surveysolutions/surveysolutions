// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignSurveyViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assign survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.Assign
{
    /// <summary>
    /// The assign survey view factory.
    /// </summary>
    public class AssignSurveyViewFactory : IViewFactory<AssignSurveyInputModel, AssignSurveyView>
    {
        #region Fields


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
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
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
        /// The RavenQuestionnaire.Core.Views.Assign.AssignSurveyView.
        /// </returns>
        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            CompleteQuestionnaireStoreDocument q = this.store.GetById(input.CompleteQuestionnaireId);

            return new AssignSurveyView(q);
        }

        #endregion
    }
}