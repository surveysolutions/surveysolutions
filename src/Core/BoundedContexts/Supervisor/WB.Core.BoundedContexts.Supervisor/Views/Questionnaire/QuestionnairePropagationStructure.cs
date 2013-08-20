using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class QuestionnairePropagationStructure : IVersionedView
    {
        public QuestionnairePropagationStructure()
        {
            PropagationScopes = new Dictionary<Guid, HashSet<Guid>>();
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, HashSet<Guid>> PropagationScopes { get; set; }
        public long Version { get; set; }
    }
}
