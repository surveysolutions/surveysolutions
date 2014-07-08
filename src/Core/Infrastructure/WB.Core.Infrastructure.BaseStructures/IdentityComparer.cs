using System.Collections.Generic;
using System.Linq;

namespace WB.Core.Infrastructure.BaseStructures
{
    public class IdentityComparer : IEqualityComparer<Identity>
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
