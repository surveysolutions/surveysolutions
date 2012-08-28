using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class PropagatedGroupMobileView : CompleteGroupMobileView
    {
        public PropagatedGroupMobileView(CompleteQuestionnaireDocument doc, ICompleteGroup group)
        {
            /* if (!group.PropogationPublicKey.HasValue)
                 throw new ArgumentException("Group is not propagated");*/
            this.PublicKey = group.PublicKey;
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Title = group.Title;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
            this.PropogationKey = group.PropogationPublicKey ?? Guid.Empty;
            this.Children =
                group.Children.OfType<ICompleteQuestion>().Select(
                    q => new CompleteQuestionFactory().CreateQuestion(doc, q) as ICompositeView).ToList();

            this.FeaturedTitle =
                string.Concat(
                    group.Children.OfType<ICompleteQuestion>().Where(q => q.Capital).Select(
                        q => q.GetAnswerString() + " "));


        }

        public PropagatedGroupMobileView(CompleteQuestionnaireDocument doc, ICompleteGroup group, ScreenNavigation navigation):this(doc,group)
        {
            this.Navigation = navigation;
        }

        public string FeaturedTitle { get; set; }
        public bool AutoPropagate { get; private set; }
        public Guid PropogationKey { get; private set; }
    }
}