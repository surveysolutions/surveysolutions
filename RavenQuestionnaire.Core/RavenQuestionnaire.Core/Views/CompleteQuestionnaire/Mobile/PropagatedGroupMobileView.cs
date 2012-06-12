using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class PropagatedGroupMobileView : CompleteGroupMobileView
    {
        public PropagatedGroupMobileView(Guid key, string text, bool isAutoPropagate, Guid propagationKey, List<CompleteQuestionView> questions)
        {
            this.PublicKey = key;
            this.Title = text;
            this.AutoPropagate = isAutoPropagate;
            this.PropogationKey = propagationKey;
            Children.AddRange(questions);
            Navigation = new ScreenNavigation();
        }

        public string FeaturedTitle { get; set; }
        public bool AutoPropagate { get; private set; }
        public Guid PropogationKey { get; private set; }
    }
}