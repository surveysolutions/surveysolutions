using Main.Core.Entities.Extensions;

namespace Main.Core.View.Group
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Question;

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
            CompleteQuestionnaireStoreDocument doc, CompleteGroup currentGroup, QuestionScope scope)
            : this()
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.PublicKey = currentGroup.PublicKey;
            this.Title = currentGroup.Title;
            this.Propagated = currentGroup.Propagated;
            this.Enabled = currentGroup.Enabled;
            this.Description = currentGroup.Description;
            this.IsQuestionnaireActive = !SurveyStatus.IsStatusAllowCapiSync(doc.Status);
            if (currentGroup.Propagated != Propagate.None)
            {
                this.PropagateTemplate = new PropagatedGroupMobileView(doc, currentGroup, scope);
            }
            else
            {
                foreach (IComposite composite in currentGroup.Children)
                {
                    if ((composite as ICompleteQuestion) != null)
                    {
                        var q = composite as ICompleteQuestion;
                        if (q.QuestionScope <= scope)
                        {
                            var question = new CompleteQuestionView(doc, q);
                            this.Children.Add(question);
                        }
                    }
                    else
                    {
                        var g = composite as CompleteGroup;
                        if (g.Propagated == Propagate.None || !g.PropagationPublicKey.HasValue)
                        {
                            this.Children.Add(new CompleteGroupMobileView(doc, g, scope));
                        }
                        else
                        {
                            ICompositeView template =
                                this.Children.FirstOrDefault(
                                    parent => parent.PublicKey == g.PublicKey && !(parent is PropagatedGroupMobileView));
                            template.Children.Add(new PropagatedGroupMobileView(doc, g, scope));
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