// -----------------------------------------------------------------------
// <copyright file="PropagatedGridViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PropagatedGridViewFactory : IViewFactory<PropagatedGridViewInputModel, PropagatedGroupGridContainer>
    {
        #region Constants and Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        public PropagatedGridViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
        }

        #region Implementation of IViewFactory<PropagatedGridViewInputModel,PropagatedGroupsContainer>

        public PropagatedGroupGridContainer Load(PropagatedGridViewInputModel input)
        {
            CompleteQuestionnaireStoreDocument doc = this.store.GetById(input.CompelteQuestionnairePublicKey);
            if (doc == null)
                return null;
            var groupTemplate =
                doc.Find<ICompleteGroup>(g => g.PublicKey == input.GroupPublicKey && !g.PropagationPublicKey.HasValue).
                    FirstOrDefault();
            if (groupTemplate == null)
                return null;
            PropagatedGroupGridContainer result = new PropagatedGroupGridContainer(doc, groupTemplate);
            foreach (
                ICompleteGroup completeGroup in
                    doc.Find<ICompleteGroup>(g => g.PublicKey == input.GroupPublicKey && g.PropagationPublicKey.HasValue)
                )
            {
                result.AddRow(doc, completeGroup);
            }
            return result;
        }
    

    #endregion
    }
}
