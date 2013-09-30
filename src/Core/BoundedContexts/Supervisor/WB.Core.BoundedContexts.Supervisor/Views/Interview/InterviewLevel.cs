using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel(Guid scopeId, int[] vector)
        {
            ScopeIds = new HashSet<Guid>(new[] {scopeId});
            PropagationVector = vector;
            Questions = new List<InterviewQuestion>();
            DisabledGroups = new HashSet<Guid>();
        }

        public int[] PropagationVector { get; private set; }
        public HashSet<Guid> ScopeIds { get; private set; }
        public List<InterviewQuestion> Questions { get; private set; }
        public HashSet<Guid> DisabledGroups { get; private set; }
    }
}
