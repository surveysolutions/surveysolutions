// -----------------------------------------------------------------------
// <copyright file="CapiScreenViewSupplier.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
using Main.Core.View.Group;

namespace Core.CAPI.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CapiScreenViewSupplier : DefaultScreenViewSupplier
    {
        #region Implementation of IScreenViewSupplier

        public override ScreenGroupView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup,
                                         ScreenNavigation navigation)
        {
            if (currentGroup.Propagated != Propagate.None && !currentGroup.PropogationPublicKey.HasValue)
            {
                return new CapiScreenGroupView(doc, currentGroup, navigation);
            }
            var baseResult = base.BuildView(doc, currentGroup, navigation);
            
            foreach (
                CompleteGroupMobileView completeGroupMobileView in
                    baseResult.Group.Children.OfType<CompleteGroupMobileView>().Where(
                        g => g.Propagated != Propagate.None /*&& g.Visualization == GroupVisualization.Grid*/).ToList())
            {
                completeGroupMobileView.Children.Clear();
                completeGroupMobileView.Propagated = Propagate.None;
            }
            if (currentGroup.PropogationPublicKey.HasValue)
            {
                baseResult.Navigation.NavigationContent.BreadCumbs.Insert(
                    baseResult.Navigation.NavigationContent.BreadCumbs.Count,
                    new CompleteGroupHeaders() {GroupText = currentGroup.Title, PublicKey = currentGroup.PublicKey});
                baseResult.Navigation.NavigationContent.CurrentScreenTitle =
                    doc.GetGroupTitle(currentGroup.PropogationPublicKey.Value);
            }
            return baseResult;
        }

        #endregion

    }
}
