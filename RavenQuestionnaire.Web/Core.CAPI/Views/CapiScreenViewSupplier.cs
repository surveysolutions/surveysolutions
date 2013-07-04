namespace Core.CAPI.Views
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CapiScreenViewSupplier : DefaultScreenViewSupplier
    {
        #region Implementation of IScreenViewSupplier

        /// <summary>
        /// The build view.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="navigation">
        /// The navigation.
        /// </param>
        /// <param name="scope">
        /// The question scope
        /// </param>
        /// <returns>
        /// The <see cref="ScreenGroupView"/>.
        /// </returns>
        public override ScreenGroupView BuildView(
            CompleteQuestionnaireStoreDocument doc,
            ICompleteGroup currentGroup,
            ScreenNavigation navigation,
            QuestionScope scope)
        {
            if (currentGroup.Propagated != Propagate.None && !currentGroup.PropagationPublicKey.HasValue)
            {
                return new CapiScreenGroupView(doc, currentGroup, navigation);
            }

            var baseResult = base.BuildView(doc, currentGroup, navigation, scope);
            
            foreach (CompleteGroupMobileView completeGroupMobileView in
                    baseResult.Group.Children.OfType<CompleteGroupMobileView>().Where(
                        g => g.Propagated != Propagate.None /*&& g.Visualization == GroupVisualization.Grid*/).ToList())
            {
                completeGroupMobileView.Children.Clear();
                completeGroupMobileView.Propagated = Propagate.None;
            }

            if (currentGroup.PropagationPublicKey.HasValue)
            {
                baseResult.Navigation.NavigationContent.BreadCumbs.Insert(
                    baseResult.Navigation.NavigationContent.BreadCumbs.Count,
                    new CompleteGroupHeaders() { GroupText = currentGroup.Title, PublicKey = currentGroup.PublicKey });
                baseResult.Navigation.NavigationContent.CurrentScreenTitle =
                    doc.GetGroupTitle(currentGroup.PropagationPublicKey.Value);
            }

            return baseResult;
        }

        #endregion

    }
}
