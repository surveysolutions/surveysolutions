using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public class Identity
    {
        public Guid Id { get; private set; }
        public decimal[] RosterVector { get; private set; }

        public Identity(Guid id, decimal[] rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}>", this.Id.FormatGuid(), string.Join("-", this.RosterVector));
        }
    }
}