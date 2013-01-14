// -----------------------------------------------------------------------
// <copyright file="SurveyScreenView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;
    using Main.Core.Utility;
    using Main.Core.View.Group;
    using Main.Core.View.Question;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SurveyScreenView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyScreenView"/> class.
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
        /// The scope.
        /// </param>
        public SurveyScreenView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigationView navigation, QuestionScope scope)
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.PublicKey = currentGroup.PublicKey;
            this.Title = currentGroup.Title;
            this.Status = doc.Status;
            this.Description = currentGroup.Description;
            this.Navigation = navigation;

            var validator = new CompleteQuestionnaireValidationExecutor(doc, scope);
            validator.Execute(currentGroup);
            this.Screens = new List<SurveyScreen>();
            this.BuildScreenContent(doc, currentGroup, scope);
        }

        /// <summary>
        /// Gets or sets Screens.
        /// </summary>
        public List<SurveyScreen> Screens { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public CompleteGroupMobileView Group { get; set; }

        /// <summary>
        /// get or set questionnaire active status - active if allow to edit, not error or completed
        /// </summary>
        public bool IsQuestionnaireActive
        {
            get { return !SurveyStatus.IsStatusAllowCapiSync(this.Status); }
        }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Navigation.
        /// </summary>
        public ScreenNavigationView Navigation { get; set; }

        /// <summary>
        /// The build screen content.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="scope">
        /// The scope.
        /// </param>
        private void BuildScreenContent(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, QuestionScope scope)
        {
            
            //if (currentGroup.PropagationPublicKey.HasValue)
            //{
            //    this.Group = new PropagatedGroupMobileView(doc, currentGroup, scope);
            //    return;
            //}

            this.Group = new CompleteGroupMobileView()
            {
                PublicKey = currentGroup.PublicKey,
                Title = currentGroup.Title,
                Propagated = currentGroup.Propagated,
                Enabled = currentGroup.Enabled,
                Description = currentGroup.Description,
                QuestionnairePublicKey = doc.PublicKey
            };
            //this.Questions =
            //    doc.QuestionHash.Questions.Where(q => q.QuestionScope <= scope).Select(
            //        q => new CompleteQuestionView(doc, q)).ToList();

            var treeStack = new Stack<NodeWithLevel>();
            treeStack.Push(new NodeWithLevel(currentGroup, 0));
            while (treeStack.Count > 0)
            {
                NodeWithLevel node = treeStack.Pop();

                if (node.Group.Propagated != Propagate.None && !node.Group.PropagationPublicKey.HasValue)
                {
                    continue;
                }

                var screen = new SurveyScreen(doc, node);
                this.Screens.Add(screen);

                ICompleteGroup[] subGroups = node.Group.Children.OfType<ICompleteGroup>().ToArray();
                for (int i = subGroups.Length - 1; i >= 0; i--)
                {
                    // questions exists, but they are hidden 
                    if (!subGroups[i].HasVisibleItemsForScope(scope))
                    {
                        continue;
                    }

                    treeStack.Push(new NodeWithLevel(subGroups[i], node.Level + 1));
                }
            }
            //foreach (IComposite composite in currentGroup.Children)
            //{
            //    if ((composite as ICompleteQuestion) != null)
            //    {
            //        var q = composite as ICompleteQuestion;
            //        if (q.QuestionScope <= scope)
            //        {
            //            var question = new CompleteQuestionView(doc, q);
            //            if (q.QuestionScope == scope)
            //            {
            //                question.Editable = true;
            //            }

            //            this.Group.Children.Add(question);
            //        }
            //    }
            //    else
            //    {
            //        var g = composite as CompleteGroup;
            //        if (g.Propagated == Propagate.None)
            //        {
            //            this.Group.Children.Add(new CompleteGroupMobileView(doc, g, scope));
            //        }
            //        else if (!g.PropagationPublicKey.HasValue)
            //        {
            //            var propagatedGroup = new CompleteGroupMobileView(doc, g, scope);
            //            this.Group.Children.Add(propagatedGroup);
            //            var subGroups = currentGroup.Children.OfType<ICompleteGroup>().Where(
            //                p =>
            //                p.PublicKey == g.PublicKey && p.PropagationPublicKey.HasValue);
            //            foreach (
            //                ICompleteGroup completeGroup in subGroups)
            //            {
            //                propagatedGroup.Children.Add(new PropagatedGroupMobileView(doc, completeGroup, scope));
            //            }
            //        }
            //    }
            //}
        }

        #endregion
    }
}
