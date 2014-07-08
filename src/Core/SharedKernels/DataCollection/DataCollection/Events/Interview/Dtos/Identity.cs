using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static Identity ToEventIdentity(Infrastructure.BaseStructures.Identity identity)
        {
            return new Identity(identity.Id, identity.RosterVector);
        }

        public static IEnumerable<Identity> ToEventIdentities(IEnumerable<Infrastructure.BaseStructures.Identity> identities)
        {
            return identities.Select(ToEventIdentity).ToArray();
        }

        public static Infrastructure.BaseStructures.Identity ToIdentity(Identity identity)
        {
            return new Infrastructure.BaseStructures.Identity(identity.Id, identity.RosterVector);
        }

        public static IEnumerable<Infrastructure.BaseStructures.Identity> ToIdentities(IEnumerable<Identity> identities)
        {
            return identities.Select(ToIdentity).ToArray();
        }
    }

    public static class IdentityExtentions
    {
        public static IEnumerable<Infrastructure.BaseStructures.Identity> ToIdentities(this IEnumerable<Identity> identities)
        {
            return Identity.ToIdentities(identities);
        }
    }
}