// -----------------------------------------------------------------------
// <copyright file="ScreenGroupView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;
using Main.Core.View.Group;
using Main.Core.View.Question;

namespace Core.CAPI.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenGroupView
    {
       #region Constructors and Destructors


        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupMobileView"/> class.
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
        public ScreenGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup,ScreenNavigation navigation)
        {
            this.QuestionnairePublicKey = doc.PublicKey;
              this.PublicKey = currentGroup.PublicKey;
            /*this.Title = currentGroup.Title;
            this.Propagated = currentGroup.Propagated;
            this.Visualization = currentGroup.Visualization;
            this.Enabled = currentGroup.Enabled;
            this.Description = currentGroup.Description;*/
            this.Navigation =
                new ScreenNavigationView(
                    doc.Children.OfType<ICompleteGroup>().Select(g => new CompleteGroupHeaders(g)), navigation);
            this.IsQuestionnaireActive = !SurveyStatus.IsStatusAllowCapiSync(doc.Status);
            if (currentGroup.Propagated != Propagate.None && !currentGroup.PropogationPublicKey.HasValue)
            {
                this.Grid = new PropagatedGroupGridContainer(doc, currentGroup);
                foreach (
                    ICompleteGroup completeGroup in
                        doc.Find<ICompleteGroup>(
                            g => g.PublicKey == currentGroup.PublicKey && g.PropogationPublicKey.HasValue)
                    )
                {
                    this.Grid.AddRow(doc, completeGroup);
                }
                return;
            }
            if (currentGroup.PropogationPublicKey.HasValue)
            {
                this.Group = new PropagatedGroupMobileView(doc, currentGroup);
                return;
            }
            this.Group = new CompleteGroupMobileView()
                {
                    PublicKey = currentGroup.PublicKey,
                    Title = currentGroup.Title,
                    Propagated = currentGroup.Propagated,
                    Visualization = currentGroup.Visualization,
                    Enabled = currentGroup.Enabled,
                    Description = currentGroup.Description,
                    QuestionnairePublicKey = doc.PublicKey
                };
            foreach (IComposite composite in currentGroup.Children)
            {
                if ((composite as ICompleteQuestion) != null)
                {
                    var q = composite as ICompleteQuestion;
                    var question = new CompleteQuestionView(doc, q);
                    this.Group.Children.Add(question);
                }
                else
                {
                    var g = composite as CompleteGroup;
                    if (g.Propagated == Propagate.None)
                    {
                        this.Group.Children.Add(new CompleteGroupMobileView(doc, g));
                    }
                    else if (!g.PropogationPublicKey.HasValue)
                    {
                        var propagatedGroup = new CompleteGroupMobileView(doc, g);
                        this.Group.Children.Add(propagatedGroup);
                        if (g.Visualization == GroupVisualization.Nested)
                        {
                            var subGroups = currentGroup.Children.OfType<ICompleteGroup>().Where(
                                p =>
                                p.PublicKey == g.PublicKey && p.PropogationPublicKey.HasValue);
                            foreach (
                                ICompleteGroup completeGroup in subGroups)
                            {
                                propagatedGroup.Children.Add(new PropagatedGroupMobileView(doc, completeGroup));
                            }

                        }
                        else
                        {
                            propagatedGroup.Propagated = Propagate.None;

                        }
                        this.Group.Children.Add(propagatedGroup);
                    }
                }
            }
        }

        #endregion

        public PropagatedGroupGridContainer Grid { get; set; }

        public CompleteGroupMobileView Group { get; set; }
        /// <summary>
        /// get or set questionnaire active status - active if allow to edit, not error or completed
        /// </summary>
        public bool IsQuestionnaireActive { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }
        public Guid PublicKey { get; set; }
        public ScreenNavigationView Navigation { get; set; }
    }
}
