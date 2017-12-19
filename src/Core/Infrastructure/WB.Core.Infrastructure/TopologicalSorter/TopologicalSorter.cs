using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.Infrastructure.TopologicalSorter
{
    /// <summary>
    /// Implementation of the Tarjan stronly connected components algorithm. 
    /// https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
    /// http://en.wikipedia.org/wiki/Topological_sorting
    /// </summary>
    public class TopologicalSorter<T> : ITopologicalSorter<T>
    {
        public List<List<T>> DetectCycles(Dictionary<T, T[]> dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentException("dependencies");
            }

            var vertices = BuildDependencyGraph(dependencies);

            var listOfConnectedComponents = new List<List<Vertex<T>>>();
            var index = 0;
            var nodeStatck = new Stack<Vertex<T>>();

            
            foreach (var vertex in vertices)
            {
                if (vertex.Index < 0)
                {
                    StrongConnect(vertex, listOfConnectedComponents, ref index, nodeStatck);
                }
            }

            return listOfConnectedComponents.Where(this.AreVericesInCycle).Select(x => x.Select(v => v.Value).ToList()).ToList();
        }

        private bool AreVericesInCycle(List<Vertex<T>> vertices)
        {
            if (vertices.Count > 1)
                return true;

            var vertex = vertices.Single();

            return vertex.Dependencies.Select(x => x.Value).Contains(vertex.Value);
        }

        private static List<Vertex<T>> BuildDependencyGraph(Dictionary<T, T[]> dependencies)
        {
            var uniqueVertexValues = dependencies.Keys.Union(dependencies.SelectMany(x => x.Value)).Distinct();

            Dictionary<T, Vertex<T>> vertices = uniqueVertexValues
                .ToDictionary(vertextValue => vertextValue, vertextValue => new Vertex<T>(vertextValue));

            foreach (var dependency in dependencies)
            {
                dependency.Value.ForEach(x => vertices[dependency.Key].Dependencies.Add(vertices[x]));
            }

            return vertices.Values.ToList();
        }

        private static void StrongConnect(Vertex<T> vertex, List<List<Vertex<T>>> listOfDetectedCycles, ref int index, Stack<Vertex<T>> nodeStatck)
        {
            vertex.Index = index;
            vertex.LowLink = index;
            index++;
            nodeStatck.Push(vertex);

            foreach (Vertex<T> dependency in vertex.Dependencies)
            {
                if (dependency.Index < 0)
                {
                    StrongConnect(dependency, listOfDetectedCycles, ref index, nodeStatck);
                    vertex.LowLink = Math.Min(vertex.LowLink, dependency.LowLink);
                }
                else if (nodeStatck.Contains(dependency))
                {
                    vertex.LowLink = Math.Min(vertex.LowLink, dependency.Index);
                }
            }

            if (vertex.LowLink != vertex.Index) return;

            List<Vertex<T>> stronglyConnectedComponents = new List<Vertex<T>>();
            Vertex<T> nodeInStronglyConnectedComponents;
            do
            {
                nodeInStronglyConnectedComponents = nodeStatck.Pop();
                stronglyConnectedComponents.Add(nodeInStronglyConnectedComponents);
            } while (vertex != nodeInStronglyConnectedComponents);

            listOfDetectedCycles.Add(stronglyConnectedComponents);
        }

        public List<T> Sort(Dictionary<T, T[]> dependencies)
        {
            return Sort(dependencies, default(T));
        }

        public List<T> Sort(Dictionary<T, T[]> dependencies, T root)
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

            if (EqualityComparer<T>.Default.Equals(root, default(T)))
                unmarkedNodes.AddRange(dependencies.Keys);
            else
                unmarkedNodes.Add(root);

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
                this.Visit(dependentItem, temporaryMarked, dependencies, unmarkedNodes, orderedItems);
            }
            temporaryMarked.Remove(item);
            orderedItems.Insert(0, item);
        }
    }
}
