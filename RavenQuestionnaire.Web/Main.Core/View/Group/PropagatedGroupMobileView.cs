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
using Main.Core.Entities.Extensions;
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
            this.PropogationKey = group.PropagationPublicKey ?? Guid.Empty;
            this.Enabled = group.Enabled;
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Title = string.Concat(doc.GetPropagatedGroupsByKey(this.PropogationKey).SelectMany(q => q.Children).
                                           OfType
                                           <ICompleteQuestion>().Where(q => q.Capital).Select(
                                               q => q.GetAnswerString() + " ")) + " " + group.Title;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
            
            this.IsQuestionnaireActive = !SurveyStatus.IsStatusAllowCapiSync(doc.Status);
            this.Description = group.Description;
            this.Children =
                group.Children.OfType<ICompleteQuestion>().Select(
                    q => new CompleteQuestionView(doc, q) as ICompositeView).ToList();

                
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether auto propagate.
        /// </summary>
        public bool AutoPropagate { get; private set; }


        /// <summary>
        /// Gets the propogation key.
        /// </summary>
        public Guid PropogationKey { get; private set; }

        #endregion
    }
}