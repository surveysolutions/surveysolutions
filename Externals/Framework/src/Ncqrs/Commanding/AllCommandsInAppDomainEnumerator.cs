using System;
using System.Collections.Generic;
using System.Linq;

namespace Ncqrs.Commanding
{
    [Obsolete("All commands should be mapped manually and known while registration")]
    public class AllCommandsInAppDomainEnumerator : IKnownCommandsEnumerator
    {
        public IEnumerable<Type> GetAllCommandTypes()
        {
            return Enumerable.Empty<Type>();
        }
    }
}
