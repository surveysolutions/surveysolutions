// -----------------------------------------------------------------------
// <copyright file="PropagatedGridViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PropagatedGridViewFactory:IViewFactory<PropagatedGridViewInputModel, PropagatedGroupsContainer>
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

        public PropagatedGroupsContainer Load(PropagatedGridViewInputModel input)
        {
            CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompelteQuestionnairePublicKey);
            if (doc == null)
                return null;
            var groupTemplate =
                doc.Find<ICompleteGroup>(g => g.PublicKey == input.GroupPublicKey && !g.PropogationPublicKey.HasValue).FirstOrDefault();
            if (groupTemplate == null)
                return null;
            PropagatedGroupsContainer result = new PropagatedGroupsContainer(groupTemplate, doc.PublicKey);
            foreach (
                ICompleteGroup completeGroup in
                    doc.Find<ICompleteGroup>(g => g.PublicKey == input.GroupPublicKey && g.PropogationPublicKey.HasValue)
                )
            {
                result.AddRow(completeGroup,
                              string.Concat(doc.GetPropagatedGroupsByKey(completeGroup.PropogationPublicKey.Value).
                                                SelectMany(q => q.Children).
                                                OfType
                                                <ICompleteQuestion>().Where(q => q.Capital).Select(
                                                    q => q.GetAnswerString() + " ")));
            }
            return result;
        }

        #endregion
    }
}
