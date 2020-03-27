using System;
using System.Linq;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SubstitutionTitlesChanged : InterviewPassiveEvent
    {
        [JsonIgnore]
        public Identity[] Questions { get; }

        [JsonIgnore]
        public Identity[] StaticTexts { get; }

        [JsonIgnore]
        public Identity[] Groups { get; }

        public SubstitutionTitlesChanged(Identity[] questions, Identity[] staticTexts, 
            Identity[] groups, DateTimeOffset originDate)
            : base(originDate)
        {
            this.Questions   = questions?.ToArray()   ?? new Identity[] {};
            this.StaticTexts = staticTexts?.ToArray() ?? new Identity[] {};
            this.Groups      = groups?.ToArray()      ?? new Identity[] {};
        }
    }
}
