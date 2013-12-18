using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterDescription
    {
        public RosterDescription(Guid scopeId, Guid? headQuestionId)
        {
            this.ScopeId = scopeId;
            this.HeadQuestionId = headQuestionId;
            this.RosterGroupsId = new HashSet<Guid>();
        }

        public Guid ScopeId { get; private set; }
        public Guid? HeadQuestionId { get; private set; }
        public HashSet<Guid> RosterGroupsId { get; private set; }
    }
}
