using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class IdentityComparer : IEqualityComparer<Identity>
    {
        private static readonly IEqualityComparer<Identity> instance = new IdentityComparer();

        public static IEqualityComparer<Identity> Instance
        {
            get { return instance; }
        }

        #region IEqualityComparer<Contact> Members

        public bool Equals(Identity x, Identity y)
        {
            return x.Id == y.Id &&
                x.RosterVector.Length == y.RosterVector.Length &&
                Enumerable.SequenceEqual<decimal>(x.RosterVector, y.RosterVector);
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
