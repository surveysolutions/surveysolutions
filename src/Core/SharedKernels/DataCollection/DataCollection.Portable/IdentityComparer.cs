using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class IdentityComparer : IEqualityComparer<Identity>
    {
        private static readonly IEqualityComparer<Identity> instance = new IdentityComparer();

        public static IEqualityComparer<Identity> Instance => instance;

        #region IEqualityComparer<Contact> Members

        public bool Equals(Identity x, Identity y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Identity obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
