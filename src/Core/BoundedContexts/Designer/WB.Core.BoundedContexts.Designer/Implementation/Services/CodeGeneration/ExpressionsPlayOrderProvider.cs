using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.TopologicalSorter;

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

        public Dictionary<Guid, List<Guid>> GetDependencyGraph(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var graph = this.expressionsGraphProvider.BuildDependencyGraph(questionnaire);
            return graph;
        }

        public Dictionary<Guid, List<Guid>> GetValidationDependencyGraph(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var graph = this.expressionsGraphProvider.BuildValidationDependencyGraph(questionnaire);
            return InvertGraph(graph);
        }

        private Dictionary<T, List<T>> InvertGraph<T>(Dictionary<T, List<T>> graph)
        {
            Dictionary<T, HashSet<T>> invertedGraph = new Dictionary<T, HashSet<T>>();

            foreach (var keyValue in graph)
            {
                foreach (var value in keyValue.Value)
                {
                    if (!invertedGraph.ContainsKey(value))
                        invertedGraph.Add(value, new HashSet<T>() { keyValue.Key });
                    else
                        invertedGraph[value].Add(keyValue.Key);
                }
            }

            return invertedGraph.ToDictionary(k => k.Key, v => v.Value.ToList());

        }
    }
}

