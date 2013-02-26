// -----------------------------------------------------------------------
// <copyright file="CapiScreenGroupView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.CAPI.Views
{
    using System;
    using System.Linq;

    using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;
    using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CapiScreenGroupView : ScreenGroupView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapiScreenGroupView"/> class.
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
        public CapiScreenGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigation navigation)
            : base(
                doc,
                currentGroup,
                new ScreenNavigationView(
                    doc.Children.OfType<ICompleteGroup>().Select(g => new CompleteGroupHeaders(g)), navigation))
        {
            this.BuildGridContent(doc, currentGroup);
        }


        public PropagatedGroupGridContainer Grid { get; set; }

        /// <summary>
        /// Build Grid Content
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        protected void BuildGridContent(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup)
        {
            if (currentGroup.Propagated != Propagate.None /* && !currentGroup.PropogationPublicKey.HasValue*/)
            {
               /* var executor = new CompleteQuestionnaireConditionExecutor(doc);
                executor.ExecuteAndChangeStateRecursive(doc, DateTime.UtcNow);*/

                var validator = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);

                this.Grid = new PropagatedGroupGridContainer(doc, currentGroup);
                
                foreach (ICompleteGroup completeGroup in doc.Find<ICompleteGroup>(g => g.PublicKey == currentGroup.PublicKey && g.PropagationPublicKey.HasValue))
                {
                    validator.Execute(completeGroup);
                    this.Grid.AddRow(doc, completeGroup);
                }
            }
        }
    }
}
