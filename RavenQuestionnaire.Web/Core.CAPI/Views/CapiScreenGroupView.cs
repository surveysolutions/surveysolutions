// -----------------------------------------------------------------------
// <copyright file="CapiScreenGroupView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.ExpressionExecutors;
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
    public class CapiScreenGroupView : ScreenGroupView
    {
        public CapiScreenGroupView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup,
                                   ScreenNavigation navigation) : base(doc, currentGroup,
                                                                       new ScreenNavigationView(
                                                                           doc.Children.OfType<ICompleteGroup>().Select(
                                                                               g => new CompleteGroupHeaders(g)),
                                                                           navigation))
        {
            BuildGridContent(doc, currentGroup);
        }


        public PropagatedGroupGridContainer Grid { get; set; }

        protected void BuildGridContent(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup)
        {
            if (currentGroup.Propagated != Propagate.None /* && !currentGroup.PropogationPublicKey.HasValue*/)
            {
                var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
                var validator = new CompleteQuestionnaireValidationExecutor(doc.QuestionHash);
                this.Grid = new PropagatedGroupGridContainer(doc, currentGroup);
                foreach (
                    ICompleteGroup completeGroup in
                        doc.Find<ICompleteGroup>(
                            g => g.PublicKey == currentGroup.PublicKey && g.PropogationPublicKey.HasValue)
                    )
                {
                    executor.Execute(completeGroup);

                    validator.Execute(completeGroup);
                    this.Grid.AddRow(doc, completeGroup);
                }
                return;
            }
        }
    }
}
