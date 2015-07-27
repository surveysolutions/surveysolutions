using System;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UserPreloadingException : Exception
    {
        public UserPreloadingException()
        {
        }

        public UserPreloadingException(string message) : base(message)
        {
        }

        public UserPreloadingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}