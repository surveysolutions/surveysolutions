using System;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] Groups { get; private set; }

        protected GroupsPassiveEvent(Identity[] groups, DateTimeOffset originDate) : base(originDate)
        {
            this.Groups = groups.ToArray();
        }
    }
}
