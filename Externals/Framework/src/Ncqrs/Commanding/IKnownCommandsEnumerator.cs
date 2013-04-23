using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Ncqrs.Commanding
{
    /// <summary>
    /// 
    /// </summary>
    #if USE_CONTRACTS
    [ContractClass(typeof(IKnownCommandsEnumeratorContracts))]
    #endif
    public interface IKnownCommandsEnumerator
    {
        IEnumerable<Type> GetAllCommandTypes();
    }
    #if USE_CONTRACTS
    [ContractClassFor(typeof(IKnownCommandsEnumerator))]
    internal abstract class IKnownCommandsEnumeratorContracts : IKnownCommandsEnumerator
    {
        public IEnumerable<Type> GetAllCommandTypes()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Type>>() != null);
            return default(IEnumerable<Type>);
        }
    }
#endif
}
