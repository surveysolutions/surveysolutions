using System;

namespace Ncqrs.Eventing.Storage
{
    public class PropertyBagConvertionException : Exception
    {
        public PropertyBagConvertionException() {}

        public PropertyBagConvertionException(string message) : base(message){ }

        public PropertyBagConvertionException(string message, Exception inner) : base(message, inner){ }
    }
}   