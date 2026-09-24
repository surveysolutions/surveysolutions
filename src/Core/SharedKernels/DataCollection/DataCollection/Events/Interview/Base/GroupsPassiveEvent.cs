using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupsPassiveEvent : InterviewPassiveEvent, IEventWithAffectedEntities
    {
        public Identity[] Groups { get; private set; }

        protected GroupsPassiveEvent(Identity[] groups, DateTimeOffset originDate) : base(originDate)
        {
            this.Groups = groups.ToArray();
        }

        IReadOnlyCollection<Identity> IEventWithAffectedEntities.GetAffectedEntities() => Groups;
    }
}
