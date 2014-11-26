using System;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public class GoneException : RestException
    {
        public GoneException() {}

        public GoneException(string message)
            : base(message) {}

        public GoneException(string message, Exception inner)
            : base(message, inner) {}
    }
}