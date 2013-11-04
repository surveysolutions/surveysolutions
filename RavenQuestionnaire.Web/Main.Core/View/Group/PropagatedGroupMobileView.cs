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

    public class PropagatedGroupMobileView : CompleteGroupMobileView
    {
        #region Constructors and Destructors

        public PropagatedGroupMobileView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, QuestionScope scope)
        {
            this.Id = group.PublicKey;
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
                }
            }
        }
        #endregion

        #region Public Properties

        public bool AutoPropagate { get; private set; }


        public Guid PropogationKey { get; private set; }

        #endregion
    }
}