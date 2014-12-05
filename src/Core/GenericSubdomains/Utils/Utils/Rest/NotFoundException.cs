using System;
using System.Runtime.Serialization;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public class NotFoundException : RestException
    {
        public NotFoundException() {}

        public NotFoundException(string message)
            : base(message) {}

        public NotFoundException(string message, Exception inner)
            : base(message, inner) {}
    }
}