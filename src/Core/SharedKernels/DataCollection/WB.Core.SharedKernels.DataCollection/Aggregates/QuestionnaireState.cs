using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public class QuestionnaireState
    {
        public QuestionnaireState(QuestionnaireDocument document, Dictionary<Guid, IQuestion> questionCache, Dictionary<Guid, IGroup> groupCache)
        {
            Document = document;
            QuestionCache = questionCache;
            GroupCache = groupCache;
        }

        public QuestionnaireDocument Document { get;private set; }
        public Dictionary<Guid, IQuestion> QuestionCache { get; private set; }
        public Dictionary<Guid, IGroup> GroupCache { get; private set; }
    }
}
