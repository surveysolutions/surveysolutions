using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.V2.CustomFunctions;

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

        public static Identity ToEventIdentity(DataCollection.Identity identity)
        {
            return new Identity(identity.Id, identity.RosterVector);
        }

        public static IEnumerable<Identity> ToEventIdentities(IEnumerable<DataCollection.Identity> identities)
        {
            return identities.Select(ToEventIdentity).ToArray();
        }

        public static DataCollection.Identity ToIdentity(Identity identity)
        {
            return new DataCollection.Identity(identity.Id, identity.RosterVector);
        }

        public static IEnumerable<DataCollection.Identity> ToIdentities(IEnumerable<Identity> identities)
        {
            return identities.Select(ToIdentity).ToArray();
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode() ^ this.RosterVector.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Identity p = obj as Identity;
            if (p == null)
                return false;

            return Equals(p);
        }

        public bool Equals(Identity p)
        {
            if (p == null)
                return false;

            return this.Id == p.Id && this.RosterVector.SequenceEqual(p.RosterVector);
        }
    }

    public static class IdentityExtentions
    {
        public static IEnumerable<DataCollection.Identity> ToIdentities(this IEnumerable<Identity> identities)
        {
            return Identity.ToIdentities(identities);
        }
    }
}