using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel(Guid scopeId)
        {
            ScopeId = scopeId;
            Questions = new List<InterviewQuestion>();
        }

        public Guid ScopeId { get; private set; }
        public IList<InterviewQuestion> Questions { get; private set; }

    }
}
