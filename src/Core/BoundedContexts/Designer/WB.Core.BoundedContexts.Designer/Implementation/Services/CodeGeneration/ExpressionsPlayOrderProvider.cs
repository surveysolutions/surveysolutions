using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services.TopologicalSorter;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class ExpressionsPlayOrderProvider : IExpressionsPlayOrderProvider
    {
        private readonly IExpressionsGraphProvider expressionsGraphProvider;

        public ExpressionsPlayOrderProvider(IExpressionsGraphProvider expressionsGraphProvider)
        {
            this.expressionsGraphProvider = expressionsGraphProvider;
        }

        public List<Guid> GetExpressionsPlayOrder(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var mergedDependencies = this.expressionsGraphProvider.BuildDependencyGraph(questionnaire);

            var sorter = new TopologicalSorter<Guid>();
            IEnumerable<Guid> lisOsfOrderedConditions = sorter.Sort(mergedDependencies.ToDictionary(x => x.Key, x => x.Value.ToArray()));
            return lisOsfOrderedConditions.ToList();
        }
    }
}

