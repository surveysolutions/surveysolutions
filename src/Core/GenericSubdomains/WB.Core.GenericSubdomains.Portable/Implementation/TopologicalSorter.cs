using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    /// <summary>
    /// Implementation of the Tarjan stronly connected components algorithm.
    /// </summary>
    /// <seealso cref="http://en.wikipedia.org/wiki/Topological_sorting"/>
    public class TopologicalSorter<T> : ITopologicalSorter<T>
    {
        public List<T> Sort(Dictionary<T, T[]> dependencies)
        {
            if (dependencies == null || dependencies.Values
            .SelectMany(dependentItems => dependentItems)
            .Any(dependentItem => !dependencies.ContainsKey(dependentItem)))
            {
                throw new ArgumentException("dependencies");
            }

            /*
L ← Empty list that will contain the sorted nodes
while there are unmarked nodes do
    select an unmarked node n
    visit(n) 
function visit(node n)
    if n has a temporary mark then stop (not a DAG)
    if n is not marked (i.e. has not been visited yet) then
        mark n temporarily
        for each node m with an edge from n to m do
            visit(m)
        mark n permanently
        unmark n temporarily
        add n to head of L
             */

            var temporaryMarked = new List<T>();
            var orderedItems = new List<T>();
            var unmarkedNodes = new List<T>();

            unmarkedNodes.AddRange(dependencies.Keys);
            while (unmarkedNodes.Any())
            {
                this.Visit(unmarkedNodes.First(), temporaryMarked, dependencies, unmarkedNodes, orderedItems);
            }

            return orderedItems;
        }

        private void Visit(T item, List<T> temporaryMarked, Dictionary<T, T[]> dependencies, List<T> unmarkedNodes, List<T> orderedItems)
        {
            if (orderedItems.Contains(item) || temporaryMarked.Contains(item)) return;

            temporaryMarked.Add(item);
            unmarkedNodes.Remove(item);

            foreach (var dependentItem in dependencies[item])
            {
                this.Visit(dependentItem, temporaryMarked, dependencies,unmarkedNodes, orderedItems);
            }
            temporaryMarked.Remove(item);
            orderedItems.Insert(0, item);
        }
    }
}
