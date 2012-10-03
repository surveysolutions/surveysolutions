// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireMabileViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire mabile view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Utility;
using Main.Core.View.Group;

namespace Main.Core.View.CompleteQuestionnaire
{
    using Main.Core.Entities.Extensions;
    using Main.Core.ExpressionExecutors;

    /// <summary>
    /// The complete questionnaire mabile view factory.
    /// </summary>
    public class CompleteQuestionnaireMabileViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireMabileViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public CompleteQuestionnaireMabileViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile.CompleteQuestionnaireMobileView.
        /// </returns>
        public CompleteQuestionnaireMobileView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId != Guid.Empty)
            {
                CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);

                this.UpdateInputData(doc, input);
                GroupWithRout rout = new GroupWithRout(doc, input.CurrentGroupPublicKey, input.PropagationKey);



                ScreenNavigation navigation = rout.Navigation;
                Guid currentScreen = navigation.BreadCumbs.Count > 1
                                         ? navigation.BreadCumbs[1].PublicKey
                                         : rout.Group.PublicKey;
                return new CompleteQuestionnaireMobileView(
                    doc, currentScreen, rout.Group, navigation);
            }

            return null;
        }

        #endregion

        #region Methods


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
        #endregion
    }
}