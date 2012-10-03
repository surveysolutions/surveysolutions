// -----------------------------------------------------------------------
// <copyright file="ScreenGroupViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.ExpressionExecutors;
using Main.Core.Utility;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.Core.View.Group;

namespace Core.CAPI.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireMobileViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public ScreenGroupViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
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
                result = new ScreenGroupView(doc, rout.Group, rout.Navigation);
            }
            return result;
        }

        #endregion
    }
}
