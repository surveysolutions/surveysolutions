using System;

namespace WB.Core.Infrastructure.Exceptions
{
    /// <summary>
    /// Thrown when binary content is not an image or its format cannot be decoded.
    /// </summary>
    public class InvalidImageException : Exception
    {
        public InvalidImageException(string message) : base(message)
        {
        }

        public InvalidImageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
