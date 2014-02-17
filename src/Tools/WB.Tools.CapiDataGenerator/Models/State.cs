using System;
using System.Collections.Generic;
using Raven.Abstractions.Extensions;

namespace CapiDataGenerator
{
    internal class State
    {
        public HashSet<Guid> FixedTitlesRosters { get; private set; }
        public Dictionary<Guid, decimal[]> RosterKeys { get; private set; }

        public State(IEnumerable<Guid> fixedTitlesRosters)
        {
            FixedTitlesRosters = fixedTitlesRosters.ToHashSet();
            this.RosterKeys = new Dictionary<Guid, decimal[]>();
        }
    }
}