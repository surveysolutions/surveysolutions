using System;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class PreloadingException : Exception
    {
        public PreloadingException()
        {
        }

        public PreloadingException(string message) : base(message)
        {
        }

        public PreloadingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
