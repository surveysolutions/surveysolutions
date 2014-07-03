using System;

namespace WB.Core.Infrastructure.BaseStructures
{
    public class Identity
    {
        // should be shared
        public Guid Id { get; private set; }
        public decimal[] RosterVector { get; private set; }
        public Identity(Guid id, decimal[] rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }
    }
}