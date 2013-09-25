using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Aggregates.Snapshots
{
    public class QuestionnaireState
    {
        public QuestionnaireDocument QuestionnaireDocument { get; set; }
        public long Version { get; set; }
    }
}
