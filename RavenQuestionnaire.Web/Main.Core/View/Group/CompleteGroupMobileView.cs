// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupMobileView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract group mobile view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;

namespace Main.Core.View.Group
{
    /// <summary>
    /// The abstract group mobile view.
    /// </summary>
    public abstract class AbstractGroupMobileView : ICompositeView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupMobileView"/> class.
        /// </summary>
        public AbstractGroupMobileView()
        {
            this.Children = new List<ICompositeView>();
            this.QuestionsWithCards = new List<CompleteQuestionView>();
            this.QuestionsWithInstructions = new List<CompleteQuestionView>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        public ScreenNavigation Navigation { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questions with cards.
        /// </summary>
        public List<CompleteQuestionView> QuestionsWithCards { get; set; }

        /// <summary>
        /// Gets or sets the questions with instructions.
        /// </summary>
        public List<CompleteQuestionView> QuestionsWithInstructions { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the unique key.
        /// </summary>
        public Guid UniqueKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }

    /// <summary>
    /// The complete group mobile view.
    /// </summary>
    public class CompleteGroupMobileView : AbstractGroupMobileView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteGroupMobileView"/> class.
        /// </summary>
        public CompleteGroupMobileView()
        {
            this.Propagated = Propagate.None;
            this.Navigation = new ScreenNavigation();
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
        public CompleteGroupMobileView(
            CompleteQuestionnaireStoreDocument doc, CompleteGroup currentGroup, ScreenNavigation navigation)
            : this()
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Navigation = navigation;
            this.PublicKey = currentGroup.PublicKey;
            this.Title = currentGroup.Title;
            this.Propagated = currentGroup.Propagated;
            this.Enabled = currentGroup.Enabled;
            this.Description = currentGroup.Description;
            this.IsQuestionnaireActive = !SurveyStatus.IsStatusAllowCapiSync(doc.Status);
            if (currentGroup.Propagated != Propagate.None)
            {
                this.PropagateTemplate = new PropagatedGroupMobileView(doc, currentGroup, navigation);
            }
            else
            {
                foreach (IComposite composite in currentGroup.Children)
                {
                    if ((composite as ICompleteQuestion) != null)
                    {
                        var q = composite as ICompleteQuestion;
                        var question = new CompleteQuestionView(doc, q);
                        this.Children.Add(question);
                    }
                    else
                    {
                        var g = composite as CompleteGroup;
                        if (g.Propagated == Propagate.None || !g.PropogationPublicKey.HasValue)
                        {
                            this.Children.Add(new CompleteGroupMobileView(doc, g, new ScreenNavigation()));
                        }
                        else
                        {
                            ICompositeView template =
                                this.Children.FirstOrDefault(
                                    parent => parent.PublicKey == g.PublicKey && !(parent is PropagatedGroupMobileView));
                            template.Children.Add(new PropagatedGroupMobileView(doc, g));
                        }
                    }

                    this.CollectGalleries(this);
                    this.CollectInstructions(this);
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// get or set questionnaire active status - active if allow to edit, not error or completed
        /// </summary>
        public bool IsQuestionnaireActive { get; set; }

        /// <summary>
        /// Gets or sets the propagate template.
        /// </summary>
        public PropagatedGroupMobileView PropagateTemplate { get; set; }

        /// <summary>
        /// Gets or sets the totals.
        /// </summary>
        public Counter Totals { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get header.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.CompleteGroupHeaders.
        /// </returns>
        public static CompleteGroupHeaders GetHeader(ICompleteGroup group)
        {
            return group == null
                       ? null
                       : new CompleteGroupHeaders { GroupText = group.Title, PublicKey = group.PublicKey };
        }

        /// <summary>
        /// The get client id.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, this.PublicKey);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The collect galleries.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        private void CollectGalleries(CompleteGroupMobileView @group)
        {
            List<CompleteQuestionView> qs = @group.Children.OfType<CompleteQuestionView>().ToList();
            if (qs.Count() > 0)
            {
                this.QuestionsWithCards.AddRange(qs.Where(question => (question.Cards.Length > 0)).ToList());
            }
        }

        /// <summary>
        /// The collect instructions.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        private void CollectInstructions(CompleteGroupMobileView @group)
        {
            List<CompleteQuestionView> qs = @group.Children.OfType<CompleteQuestionView>().ToList();
            if (qs.Count > 0)
            {
                this.QuestionsWithInstructions.AddRange(
                    qs.Where(question => !string.IsNullOrWhiteSpace(question.Instructions)).ToList());
            }
        }

        #endregion
    }
}