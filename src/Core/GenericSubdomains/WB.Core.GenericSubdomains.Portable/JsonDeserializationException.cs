using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public class JsonDeserializationException: Exception
    {
        public JsonDeserializationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
