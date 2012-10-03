// -----------------------------------------------------------------------
// <copyright file="ScreenGroupView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Group;
using Main.Core.View.Question;

namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenGroupView
    {
       #region Constructors and Destructors
        protected ScreenGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigationView navigation)
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.PublicKey = currentGroup.PublicKey;
            this.Title = currentGroup.Title;
            this.Status = doc.Status;
            this.Description = currentGroup.Description;
            this.Navigation = navigation;
        }

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
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigation navigation)
            : this(doc, currentGroup,
                new ScreenNavigationView(
                doc.Children.OfType<ICompleteGroup>().Select(g => new CompleteGroupHeaders(g)), navigation))
        {

            BuildScreenContent(doc, currentGroup);

        }

        protected void BuildScreenContent(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup)
        {
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
                        var subGroups = currentGroup.Children.OfType<ICompleteGroup>().Where(
                            p =>
                            p.PublicKey == g.PublicKey && p.PropogationPublicKey.HasValue);
                        foreach (
                            ICompleteGroup completeGroup in subGroups)
                        {
                            propagatedGroup.Children.Add(new PropagatedGroupMobileView(doc, completeGroup));
                        }

                    }
                }
            }
        }

        #endregion
        public CompleteGroupMobileView Group { get; set; }
        /// <summary>
        /// get or set questionnaire active status - active if allow to edit, not error or completed
        /// </summary>
        public bool IsQuestionnaireActive
        {
            get { return !SurveyStatus.IsStatusAllowCapiSync(Status); }
        }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }
        public Guid PublicKey { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public SurveyStatus Status { get; set; }
        public ScreenNavigationView Navigation { get; set; }
    }
}
