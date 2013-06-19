// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenGroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
    public class ScreenGroupViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, ScreenGroupView>
    {
        #region Constants and Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> store;

        /// <summary>
        /// The screen view supplier.
        /// </summary>
        private readonly IScreenViewSupplier screenViewSupplier;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenGroupViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        /// <param name="screenViewSupplier">
        /// The screen view supplier.
        /// </param>
        public ScreenGroupViewFactory(IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> store, IScreenViewSupplier screenViewSupplier)
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
        public ScreenGroupView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId == Guid.Empty)
            {
                return null;
            }
            
            CompleteQuestionnaireStoreDocument doc = this.store.GetById(input.CompleteQuestionnaireId);
            
            if (doc == null)
            {
                return null;
            }

            this.UpdateInputData(doc, input);
            /*var executor = new CompleteQuestionnaireConditionExecutor(doc);
            executor.ExecuteAndChangeStateRecursive(doc, DateTime.UtcNow);*/

            GroupWithRout rout = new GroupWithRout(doc, input.CurrentGroupPublicKey, input.PropagationKey, input.Scope);

            return this.screenViewSupplier.BuildView(doc, rout.Group, rout.Navigation, input.Scope);
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
        protected void UpdateInputData(CompleteQuestionnaireStoreDocument doc, CompleteQuestionnaireViewInputModel input)
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
