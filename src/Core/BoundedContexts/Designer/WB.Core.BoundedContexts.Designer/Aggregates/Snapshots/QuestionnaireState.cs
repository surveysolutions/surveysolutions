using System;
using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Aggregates.Snapshots
{
    [Obsolete("Looks like no need any more")]
    public class QuestionnaireState
    {
        public QuestionnaireDocument? QuestionnaireDocument { get; set; }
        public long Version { get; set; }
        public bool WasExpressionsMigrationPerformed { get; set; }
        public HashSet<Guid>? ReadOnlyUsers { get; set; }
        public HashSet<Guid>? MacroIds { get; set; }
        public HashSet<Guid>? LookupTableIds { get; set; }
    }
}
