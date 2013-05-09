// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenGroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.Group;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DisplaySurveyViewFactory : IViewFactory<DisplayViewInputModel, SurveyScreenView>
    {
        #region Constants and Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        /// <summary>
        /// The screen view supplier.
        /// </summary>
        private readonly ISurveyScreenSupplier surveyScreenSupplier;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaySurveyViewFactory"/> class. 
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        /// <param name="surveyScreenSupplier">
        /// The screen view supplier.
        /// </param>
        public DisplaySurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store, ISurveyScreenSupplier surveyScreenSupplier)
        {
            this.store = store;
            this.surveyScreenSupplier = surveyScreenSupplier;
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
        /// The SurveyScreenView.
        /// </returns>
        public SurveyScreenView Load(DisplayViewInputModel input)
        {
            if (input.CompleteQuestionnaireId == Guid.Empty)
            {
                return null;
            }

            var doc = this.store.GetById(input.CompleteQuestionnaireId);

            if (doc == null)
            {
                return null;
            }

            if (!input.CurrentGroupPublicKey.HasValue)
            {
                input.CurrentGroupPublicKey = doc.PublicKey;
            }

            var rout = new ScreenWithRout(doc, input.CurrentGroupPublicKey, input.PropagationKey, QuestionScope.Supervisor);

            var screenView = new ScreenNavigationView(rout.MenuItems, rout.Navigation);

            var result = this.surveyScreenSupplier.BuildView(doc, rout.Group, screenView);
            
            result.User = input.User;

            return result;
        }

        #endregion
    }
}
