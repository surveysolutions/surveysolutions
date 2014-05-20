using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    /// <summary>
    /// Full identity of group or question: id and roster vector.
    /// </summary>
    /// <remarks>
    /// Is used only internally to simplify return of id and roster vector as return value
    /// and to reduce parameters count in calculation methods.
    /// Should not be made public or be used in any form in events or commands.
    /// </remarks>
    [DebuggerDisplay("Id = {Id}, RosterVector = {RosterVector}")]
    internal class Identity
    {
        public Guid Id { get; private set; }

        public decimal[] RosterVector { get; private set; }

        public Identity(Guid id, decimal[] rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }

        public static Events.Interview.Dtos.Identity ToEventIdentity(Identity identity)
        {
            return new Events.Interview.Dtos.Identity(identity.Id, identity.RosterVector);
        }

        public static Events.Interview.Dtos.Identity[] ToEventIdentities(IEnumerable<Identity> answersDeclaredValid)
        {
            return answersDeclaredValid.Select(ToEventIdentity).ToArray();
        }
    }

    internal class IdentityComparer : IEqualityComparer<Identity>
    {
        #region IEqualityComparer<Contact> Members

        public bool Equals(Identity x, Identity y)
        {
            return x.Id == y.Id && x.RosterVector.SequenceEqual(y.RosterVector);
        }

        public int GetHashCode(Identity obj)
        {
            int hc = obj.RosterVector.Length;
            for (int i = 0; i < obj.RosterVector.Length; ++i)
            {
                hc = unchecked(hc * 13 + obj.RosterVector[i].GetHashCode());
            }

            return hc + obj.Id.GetHashCode() * 29;
        }

        #endregion
    }
}
