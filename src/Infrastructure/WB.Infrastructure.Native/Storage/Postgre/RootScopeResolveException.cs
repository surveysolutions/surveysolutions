using System;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class RootScopeResolveException : Exception
    {
        public RootScopeResolveException()
        {
        }

        public RootScopeResolveException(string message) : base(message)
        {
        }

        public RootScopeResolveException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
