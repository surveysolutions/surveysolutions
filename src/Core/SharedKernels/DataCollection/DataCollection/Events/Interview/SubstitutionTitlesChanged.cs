using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SubstitutionTitlesChanged : QuestionsPassiveEvent
    {
        public Identity[] StaticTexts { get; }

        public Identity[] Groups { get; set; }

        public SubstitutionTitlesChanged(Identity[] questions, Identity[] staticTexts, 
            Identity[] groups, DateTimeOffset originDate)
            : base(questions, originDate)
        {
            this.StaticTexts = staticTexts?.ToArray() ?? new Identity[]{};
            this.Groups = groups?.ToArray() ?? new Identity[] {};
        }
    }
}
