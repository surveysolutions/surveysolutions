using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    /// <remarks>
    /// We serialize this class nowhere. 
    /// After sync CAPI calculates rosterGroupInstanceIds on the fly from sync packages. 
    /// Supervisor gets bunch of events and grabs roster information from them.
    /// </remarks>
    public class DistinctDecimalList : IEnumerable<decimal>
    {
        private readonly List<decimal> list = new List<decimal>();

        public DistinctDecimalList() { }

        public DistinctDecimalList(IEnumerable<decimal> decimals)
            : this()
        {
            this.list.AddRange(decimals.Distinct());
        }

        public void Add(decimal value)
        {
            if (!this.list.Contains(value))
            {
                this.list.Add(value);
            }
        }

        public void Remove(decimal value)
        {
            this.list.Remove(value);
        }

        public bool Contains(decimal value)
        {
            return this.list.Contains(value);
        }

        public IEnumerator<decimal> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.list).GetEnumerator();
        }
    }
}
