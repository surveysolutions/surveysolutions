// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatedGroupMobileView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The propagated group mobile view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;

namespace Main.Core.View.Group
{
    /// <summary>
    /// The propagated group mobile view.
    /// </summary>
    public class PropagatedGroupMobileView : CompleteGroupMobileView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropagatedGroupMobileView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        public PropagatedGroupMobileView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
            /* if (!group.PropogationPublicKey.HasValue)
                 throw new ArgumentException("Group is not propagated");*/
            this.PublicKey = group.PublicKey;
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Title = group.Title;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
            this.PropogationKey = group.PropogationPublicKey ?? Guid.Empty;
            this.IsQuestionnaireActive = !SurveyStatus.IsStatusAllowCapiSync(doc.Status);
            this.Children =
                group.Children.OfType<ICompleteQuestion>().Select(
                    q => new CompleteQuestionView(doc, q) as ICompositeView).ToList();

            this.FeaturedTitle =
                string.Concat(
                    group.Children.OfType<ICompleteQuestion>().Where(q => q.Capital).Select(
                        q => q.GetAnswerString() + " "));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropagatedGroupMobileView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="navigation">
        /// The navigation.
        /// </param>
        public PropagatedGroupMobileView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, ScreenNavigation navigation)
            : this(doc, group)
        {
            this.Navigation = navigation;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether auto propagate.
        /// </summary>
        public bool AutoPropagate { get; private set; }

        /// <summary>
        /// Gets or sets the featured title.
        /// </summary>
        public string FeaturedTitle { get; set; }

        /// <summary>
        /// Gets the propogation key.
        /// </summary>
        public Guid PropogationKey { get; private set; }

        #endregion
    }
}