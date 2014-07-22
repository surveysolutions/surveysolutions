using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils.Implementation.TopologicalSorter;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    /// <summary>
    /// Implementation of the Tarjan stronly connected components algorithm.
    /// </summary>
    /// <seealso cref="http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm"/>
    /// <seealso cref="http://stackoverflow.com/questions/261573/best-algorithm-for-detecting-cycles-in-a-directed-graph"/>
    public class TopologicalSorter<T> : ITopologicalSorter<T>
    {
        private StronglyConnectedComponentList<T> stronglyConnectedComponents;
        private Stack<Vertex<T>> stack;
        private int index;

        public List<T> Sort(Dictionary<T, T[]> dependencies)
        {
            if (dependencies == null || dependencies.Values
                .SelectMany(dependentItems => dependentItems)
                .Any(dependentItem => !dependencies.ContainsKey(dependentItem)))
            {
                throw new ArgumentException("dependencies");
            }

            var vertexMap = dependencies.Keys.ToDictionary(key => key, key => new Vertex<T>(key));
            var graph = new List<Vertex<T>>();
            foreach (var dependency in dependencies)
            {
                var vertex = vertexMap[dependency.Key];
                foreach (var dependentItemKey in dependency.Value)
                {
                    vertex.Dependencies.Add(vertexMap[dependentItemKey]);
                }
                graph.Add(vertex);
            }

            this.stronglyConnectedComponents = new StronglyConnectedComponentList<T>();
            this.index = 0;
            this.stack = new Stack<Vertex<T>>();
            foreach (var v in graph)
            {
                if (v.Index < 0)
                {
                    this.StrongConnect(v);
                }
            }

            List<T> result = graph.OrderBy(x => x.Index).Select(x => x.Value).ToList();

            return result;
        }

        private void StrongConnect(Vertex<T> v)
        {
            v.Index = this.index;
            v.LowLink = this.index;
            this.index++;
            this.stack.Push(v);

            foreach (Vertex<T> w in v.Dependencies)
            {
                if (w.Index < 0)
                {
                    this.StrongConnect(w);
                    v.LowLink = Math.Min(v.LowLink, w.LowLink);
                }
                else if (this.stack.Contains(w))
                {
                    v.LowLink = Math.Min(v.LowLink, w.Index);
                }
            }

            if (v.LowLink == v.Index)
            {
                var scc = new StronglyConnectedComponent<T>();
                Vertex<T> w;
                do
                {
                    w = this.stack.Pop();
                    scc.Add(w);
                } while (v != w);
                this.stronglyConnectedComponents.Add(scc);
            }
        }
    }
}
