using System;
using System.Runtime.Serialization;

namespace WB.UI.Designer.Code.Attributes
{
    [Serializable]
    public class UnauthorizedException : Exception
    {
        public int ResponseStatusCode { get; }

        public UnauthorizedException()
        {
        }

        public UnauthorizedException(string message, int responseStatusCode) : base(message)
        {
            ResponseStatusCode = responseStatusCode;
        }

        public UnauthorizedException(string message, int responseStatusCode, Exception inner) : base(message, inner)
        {
            ResponseStatusCode = responseStatusCode;
        }        
    }
}
