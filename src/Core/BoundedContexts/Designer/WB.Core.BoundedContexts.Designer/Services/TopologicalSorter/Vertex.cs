﻿using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Services.TopologicalSorter
{
    internal class Vertex<T>
    {
        public Vertex()
        {
            this.Index = -1;
            this.Dependencies = new List<Vertex<T>>();
        }

        public Vertex(T value)
            : this()
        {
            this.Value = value;
        }

        public Vertex(IEnumerable<Vertex<T>> dependencies)
        {
            this.Index = -1;
            this.Dependencies = dependencies.ToList();
        }

        public Vertex(T value, IEnumerable<Vertex<T>> dependencies)
            : this(dependencies)
        {
            this.Value = value;
        }

        internal int Index { get; set; }

        internal int LowLink { get; set; }

        public T Value { get; set; }

        public ICollection<Vertex<T>> Dependencies { get; set; }

        public override string ToString()
        {
            var dependencies = string.Join(", ", this.Dependencies.Select(x => x.Value));
            return $"{this.Value}: {dependencies}";
        }
    }
}
