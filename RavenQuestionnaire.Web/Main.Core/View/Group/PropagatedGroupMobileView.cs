using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Question;

namespace Main.Core.View.Group
{
    using System.Collections.Generic;

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
        public PropagatedGroupMobileView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, QuestionScope scope)
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
            this.Children = new List<ICompositeView>();
            foreach (var q in group.Children.OfType<ICompleteQuestion>())
            {
                if (q.QuestionScope <= scope)
                {
                    var question = new CompleteQuestionView(doc, q);
                    if (q.QuestionScope == scope)
                    {
                        question.Editable = true;
                    }

                    this.Children.Add(question);
                }
            }
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