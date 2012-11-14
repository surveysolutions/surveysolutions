// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenGroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;
    using Main.Core.Utility;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DisplaySurveyViewFactory : IViewFactory<DisplaViewInputModel, ScreenGroupView>
    {
        #region Constants and Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        /// <summary>
        /// The screen view supplier.
        /// </summary>
        private readonly IScreenViewSupplier screenViewSupplier;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaySurveyViewFactory"/> class. 
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        /// <param name="screenViewSupplier">
        /// The screen view supplier.
        /// </param>
        public DisplaySurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store, IScreenViewSupplier screenViewSupplier)
        {
            this.store = store;
            this.screenViewSupplier = screenViewSupplier;
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile.CompleteGroupMobileView.
        /// </returns>
        public ScreenGroupView Load(DisplaViewInputModel input)
        {
            if (input.CompleteQuestionnaireId == Guid.Empty)
            {
                return null;
            }

            CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);

            if (doc == null)
            {
                return null;
            }

            if (!input.CurrentGroupPublicKey.HasValue)
            {
                var firstGroup = doc.Children.OfType<ICompleteGroup>().FirstOrDefault();
                if (firstGroup == null)
                {
                    return null;
                }

                input.CurrentGroupPublicKey = firstGroup.PublicKey;
            }

            var executor = new CompleteQuestionnaireConditionExecutor(doc);
            executor.ExecuteAndChangeStateRecursive(doc);

            var rout = new GroupWithRout(doc, input.CurrentGroupPublicKey, input.PropagationKey);

            var result = this.screenViewSupplier.BuildView(doc, rout.Group, rout.Navigation);
            result.Title = doc.Title;

            return result;
        }

        #endregion
    }
}
