// -----------------------------------------------------------------------
// <copyright file="ScreenGroupViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.ExpressionExecutors;
using Main.Core.Utility;

namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenGroupViewFactory: 
        IViewFactory<CompleteQuestionnaireViewInputModel, ScreenGroupView>
    {
         #region Constants and Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        private readonly IScreenViewSupplier sreenViewSupplier;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireMobileViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public ScreenGroupViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store, IScreenViewSupplier sreenViewSupplier)
        {
            this.store = store;
            this.sreenViewSupplier = sreenViewSupplier;
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
        public ScreenGroupView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId == Guid.Empty)
            {
                return null;
            }
            CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);
            UpdateInputData(doc, input);

            GroupWithRout rout = new GroupWithRout(doc, input.CurrentGroupPublicKey, input.PropagationKey);


            var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
            executor.Execute(rout.Group);
            var validator = new CompleteQuestionnaireValidationExecutor(doc.QuestionHash);
            validator.Execute(rout.Group);
            return this.sreenViewSupplier.BuildView(doc, rout.Group, rout.Navigation);
        }

        #endregion

        /// <summary>
        /// The update input data.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        protected void UpdateInputData(
            CompleteQuestionnaireStoreDocument doc, CompleteQuestionnaireViewInputModel input)
        {
            if (input.CurrentGroupPublicKey.HasValue)
            {
                return;
            }

            if (doc.LastVisitedGroup == null)
            {
                return;
            }

            input.CurrentGroupPublicKey = doc.LastVisitedGroup.GroupKey;
            input.PropagationKey = doc.LastVisitedGroup.PropagationKey;
        }

    }
}
