using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupsPassiveEvent : InterviewPassiveEvent
    {
        public Dtos.Identity[] Groups { get; private set; }

        protected GroupsPassiveEvent(Dtos.Identity[] groups)
        {
            this.Groups = groups.ToArray();
        }
    }
}