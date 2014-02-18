using System;
using System.Collections.Generic;
using Raven.Abstractions.Extensions;

namespace CapiDataGenerator
{
    internal class State
    {
        public HashSet<Guid> FixedTitlesRosters { get; private set; }
        public Dictionary<Guid, int> NumericRosterInstanceCounts { get; private set; }

        public State(IEnumerable<Guid> fixedTitlesRosters)
        {
            this.FixedTitlesRosters = fixedTitlesRosters.ToHashSet();
            this.NumericRosterInstanceCounts = new Dictionary<Guid, int>();
        }
    }
}