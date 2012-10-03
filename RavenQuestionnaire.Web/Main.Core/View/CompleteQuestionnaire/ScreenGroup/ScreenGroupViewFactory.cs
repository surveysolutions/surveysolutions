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
            ScreenGroupView result = null;
            if (input.CompleteQuestionnaireId != Guid.Empty)
            {
                CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);
                GroupWithRout rout = new GroupWithRout(doc,input.CurrentGroupPublicKey, input.PropagationKey);


                var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
                executor.Execute(rout.Group);
                var validator = new CompleteQuestionnaireValidationExecutor(doc.QuestionHash);
                validator.Execute(rout.Group);
                result = this.sreenViewSupplier.BuildView(doc, rout.Group, rout.Navigation);
            }
            return result;
        }

        #endregion
    }
}
