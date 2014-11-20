using System;
using System.Collections.Generic;

namespace Ncqrs.Commanding
{
    public interface IKnownCommandsEnumerator
    {
        IEnumerable<Type> GetAllCommandTypes();
    }
}
