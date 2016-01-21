using System;
using WB.Infrastructure.Security;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Security
{
    public class DefaultCypherManager : ICypherManager
    {
        public bool EncryptionEnforced()
        {
            return false;
        }

        public string GetPassword()
        {
            throw new NotImplementedException();
        }

        public void SetEncryptionEnforcement(bool value)
        {
            throw new NotImplementedException();
        }

        public void RegeneratePassword()
        {
            throw new NotImplementedException();
        }
    }
}